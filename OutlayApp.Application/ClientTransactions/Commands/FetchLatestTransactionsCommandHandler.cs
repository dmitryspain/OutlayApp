using MediatR;
using Microsoft.Extensions.Logging;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.LogoReferences;
using OutlayApp.Application.Monobank;
using OutlayApp.Application.Security;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Commands;

/// <summary>Pulls the newest statement of a card (the last month at most, re-reading the last day).</summary>
public class FetchLatestTransactionsCommandHandler : ICommandHandler<FetchLatestTransactionsCommand>
{
    private const int MaxDaysPeriod = 30;
    private const int FirstLogosFetchCount = 10;
    /// <summary>re-read so anything a webhook missed is picked up; duplicates are skipped</summary>
    private static readonly TimeSpan Overlap = TimeSpan.FromDays(1);

    private readonly IMonobankClient _monobank;
    private readonly StatementImporter _importer;
    private readonly ITokenProtector _protector;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly ILogger<FetchLatestTransactionsCommandHandler> _logger;
    private readonly IClientCardsRepository _cardsRepository;
    private readonly IClientTransactionRepository _transactionRepository;
    private readonly IClientRepository _clientRepository;

    public FetchLatestTransactionsCommandHandler(IMonobankClient monobank, StatementImporter importer,
        ITokenProtector protector, IClientCardsRepository cardsRepository,
        IClientTransactionRepository transactionRepository, IClientRepository clientRepository, IUnitOfWork unitOfWork,
        ISender sender, ILogger<FetchLatestTransactionsCommandHandler> logger)
    {
        _monobank = monobank;
        _importer = importer;
        _protector = protector;
        _unitOfWork = unitOfWork;
        _sender = sender;
        _logger = logger;
        _cardsRepository = cardsRepository;
        _transactionRepository = transactionRepository;
        _clientRepository = clientRepository;
    }

    public async Task<Result> Handle(FetchLatestTransactionsCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardsRepository.GetById(request.CardId, cancellationToken);
        if (card is null)
            return Result.Failure(new Error("ClientCard.NotFound", "Картку не знайдено."));
        var client = await _clientRepository.GetById(card.ClientId, cancellationToken);
        if (client?.EncryptedToken is null)
            return Result.Failure(new Error("Client.NotFound", "Клієнта не знайдено."));

        var now = DateTime.UtcNow;
        var earliestAllowed = now.AddDays(-MaxDaysPeriod);
        var latest = await _transactionRepository.GetLatest(card.Id, cancellationToken);
        var from = latest is null ? earliestAllowed : latest.DateOccured - Overlap;
        if (from < earliestAllowed)
            from = earliestAllowed;

        List<MonobankTransaction> items;
        try
        {
            items = await _monobank.GetStatement(_protector.Unprotect(client.EncryptedToken), card.ExternalCardId,
                new DateTimeOffset(from).ToUnixTimeSeconds(), new DateTimeOffset(now).ToUnixTimeSeconds(),
                cancellationToken);
        }
        catch (MonobankException ex)
        {
            _logger.LogWarning(ex, "Statement for card {CardId} not fetched", card.Id);
            return Result.Failure(MonobankErrors.From(ex));
        }

        var result = await _importer.Import(card, items, cancellationToken);

        var frequent = items
            .Where(x => x.Mcc != MccsConstants.MoneyTransfer)
            .GroupBy(x => x.Description)
            .OrderByDescending(x => x.Count())
            .Take(FirstLogosFetchCount)
            .Select(x => x.Key)
            .ToList();
        await _sender.Send(new FetchMostFrequencyIconsCommand(frequent), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Card {CardId}: {Added} new, {Updated} updated, {Removed} duplicates removed",
            card.Id, result.Added.Count, result.Updated, result.RemovedDuplicates);
        return Result.Success();
    }
}
