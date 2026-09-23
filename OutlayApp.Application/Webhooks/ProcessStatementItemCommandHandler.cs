using Microsoft.Extensions.Logging;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Live;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Webhooks;

/// <summary>A transaction pushed by Monobank: store it, update the balance and tell the open tabs.</summary>
public class ProcessStatementItemCommandHandler : ICommandHandler<ProcessStatementItemCommand>
{
    private readonly IClientCardsRepository _cardsRepository;
    private readonly StatementImporter _importer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILiveEvents _liveEvents;
    private readonly ILogger<ProcessStatementItemCommandHandler> _logger;

    public ProcessStatementItemCommandHandler(IClientCardsRepository cardsRepository, StatementImporter importer,
        IUnitOfWork unitOfWork, ILiveEvents liveEvents, ILogger<ProcessStatementItemCommandHandler> logger)
    {
        _cardsRepository = cardsRepository;
        _importer = importer;
        _unitOfWork = unitOfWork;
        _liveEvents = liveEvents;
        _logger = logger;
    }

    public async Task<Result> Handle(ProcessStatementItemCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardsRepository.GetByExternalId(request.Account, cancellationToken);
        if (card is null)
        {
            // an account the client never added (e.g. a jar) — nothing to do
            _logger.LogInformation("Webhook for unknown account {Account} ignored", request.Account);
            return Result.Success();
        }

        var result = await _importer.Import(card, new[] { request.Item }, cancellationToken);
        card.UpdateBalance(request.Item.ToDetails().BalanceAfter);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var t in result.Added)
        {
            await _liveEvents.Publish(card.Id, new LiveEvent(LiveEventTypes.Transaction,
                new LiveTransaction(t.Description, t.Amount, t.DateOccured, t.Mcc, t.BalanceAfter, t.CounterName, t.Hold)));
        }

        return Result.Success();
    }
}
