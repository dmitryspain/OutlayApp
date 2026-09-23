using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Monobank;
using OutlayApp.Application.Security;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Backfill;

/// <summary>One statement request of a history job. Monobank errors are thrown, so the worker can reschedule.</summary>
public class ImportStatementWindowCommandHandler : ICommandHandler<ImportStatementWindowCommand, ImportWindowResult>
{
    private readonly IClientCardsRepository _cardsRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IMonobankClient _monobank;
    private readonly ITokenProtector _protector;
    private readonly StatementImporter _importer;
    private readonly IUnitOfWork _unitOfWork;

    public ImportStatementWindowCommandHandler(IClientCardsRepository cardsRepository,
        IClientRepository clientRepository, IMonobankClient monobank, ITokenProtector protector,
        StatementImporter importer, IUnitOfWork unitOfWork)
    {
        _cardsRepository = cardsRepository;
        _clientRepository = clientRepository;
        _monobank = monobank;
        _protector = protector;
        _importer = importer;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportWindowResult>> Handle(ImportStatementWindowCommand request,
        CancellationToken cancellationToken)
    {
        var card = await _cardsRepository.GetById(request.CardId, cancellationToken);
        if (card is null)
            return Result.Failure<ImportWindowResult>(new Error("ClientCard.NotFound", "Картку не знайдено."));
        var client = await _clientRepository.GetById(card.ClientId, cancellationToken);
        if (client?.EncryptedToken is null)
            return Result.Failure<ImportWindowResult>(new Error("Client.NotFound", "Клієнта не знайдено."));

        var from = Math.Max(request.Floor, request.To - MonobankConstants.MaxStatementSeconds + 1);
        var items = await _monobank.GetStatement(_protector.Unprotect(client.EncryptedToken), card.ExternalCardId,
            from, request.To, cancellationToken);
        var result = await _importer.Import(card, items, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // a full page means the window holds more: continue just before its oldest item
        var nextTo = items.Count >= MonobankConstants.StatementPageSize
            ? items.Min(x => x.Time) - 1
            : from - 1;
        return Result.Success(new ImportWindowResult(result.Added.Count, nextTo, nextTo <= request.Floor));
    }
}
