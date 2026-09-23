using MediatR;
using Microsoft.Extensions.Logging;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.LogoReferences;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Commands;

public class FetchLatestTransactionsCommandHandler : ICommandHandler<FetchLatestTransactionsCommand>
{
    #region Properties


    private const int MaxDaysPeriod = 30;
    private const int FirstLogosFetchCount = 10;
    private const int OverlapDays = 1;
    private readonly StatementImporter _importer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly ILogger<FetchLatestTransactionsCommandHandler> _logger;
    private readonly IClientCardsRepository _cardsRepository;
    private readonly IClientTransactionRepository _transactionRepository;
    private readonly IClientRepository _clientRepository;

    #endregion

    public FetchLatestTransactionsCommandHandler(StatementImporter importer, IClientCardsRepository cardsRepository,
        IClientTransactionRepository transactionRepository, IClientRepository clientRepository, IUnitOfWork unitOfWork,
        ISender sender, ILogger<FetchLatestTransactionsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _sender = sender;
        _logger = logger;
        _cardsRepository = cardsRepository;
        _transactionRepository = transactionRepository;
        _clientRepository = clientRepository;
        _importer = importer;
    }

    public async Task<Result> Handle(FetchLatestTransactionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clientCard = await _cardsRepository.GetById(request.CardId, cancellationToken);
            if (clientCard is null)
                return Result.Failure(new Error("ClientCard.NotFound",
                    $"No client card with External Id {request.CardId}"));

            var now = DateTimeOffset.Now;
            var earliestAllowed = now.AddDays(-MaxDaysPeriod);
            var latest = await _transactionRepository.GetLatest(clientCard.Id, cancellationToken);
            // re-read the last day as well: anything a webhook missed gets picked up, duplicates are skipped
            var from = latest is null
                ? earliestAllowed
                : new DateTimeOffset(latest.DateOccured).AddDays(-OverlapDays);
            if (from < earliestAllowed)
                from = earliestAllowed;

            var client = await _clientRepository.GetById(clientCard.ClientId, cancellationToken);
            var monobankTransactions = await _importer.FetchPage(client.PersonalToken, clientCard.ExternalCardId,
                from.ToUnixTimeSeconds(), now.ToUnixTimeSeconds(), cancellationToken);

            var added = await _importer.AddNew(clientCard, monobankTransactions, cancellationToken);

            var mostFrequencyTransactions = monobankTransactions
                .Where(x => x.Mcc != MccsConstants.MoneyTransfer) 
                .GroupBy(x => x.Description)
                .OrderByDescending(x => x.Count())
                .Take(FirstLogosFetchCount)
                .Select(x => x.Key);

            var freqLogosCommand = new FetchMostFrequencyIconsCommand(mostFrequencyTransactions);
            
            await _sender.Send(freqLogosCommand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Fetched {Count} new transactions for card {CardId}", added.Count, clientCard.Id);
        }
        catch (MonobankRateLimitException ex)
        {
            _logger.LogWarning(ex, "Statement request throttled by Monobank");
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error");
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
        }
        
        return Result.Success();
    }
}
