using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Backfill;

public class ImportStatementWindowCommandHandler : ICommandHandler<ImportStatementWindowCommand, ImportWindowResult>
{
    private readonly IClientCardsRepository _cardsRepository;
    private readonly IClientRepository _clientRepository;
    private readonly StatementImporter _importer;
    private readonly IUnitOfWork _unitOfWork;

    public ImportStatementWindowCommandHandler(IClientCardsRepository cardsRepository,
        IClientRepository clientRepository, StatementImporter importer, IUnitOfWork unitOfWork)
    {
        _cardsRepository = cardsRepository;
        _clientRepository = clientRepository;
        _importer = importer;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportWindowResult>> Handle(ImportStatementWindowCommand request,
        CancellationToken cancellationToken)
    {
        var card = await _cardsRepository.GetById(request.CardId, cancellationToken);
        if (card is null)
            return Result.Failure<ImportWindowResult>(new Error("ClientCard.NotFound", "No such card"));
        var client = await _clientRepository.GetById(card.ClientId, cancellationToken);

        var from = Math.Max(request.Floor, request.To - MonobankConstants.MaxStatementSeconds + 1);
        var items = await _importer.FetchPage(client.PersonalToken, card.ExternalCardId, from, request.To,
            cancellationToken);
        var added = await _importer.AddNew(card, items, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // a full page means the window holds more: continue just before its oldest item
        var nextTo = items.Count >= MonobankConstants.StatementPageSize
            ? items.Min(x => (long)x.Time) - 1
            : from - 1;
        return Result.Success(new ImportWindowResult(added.Count, nextTo, nextTo <= request.Floor));
    }
}
