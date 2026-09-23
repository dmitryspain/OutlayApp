using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Backfill;

/// <summary>Load older history: from where the stored history begins back to <paramref name="Months"/> months ago.</summary>
public sealed record StartBackfillCommand(Guid CardId, int Months) : ICommand<BackfillStatus>;

/// <summary>Re-read the last <paramref name="Days"/> days: fills gaps, gives old rows their bank ids, drops duplicates.</summary>
public sealed record StartResyncCommand(Guid CardId, int Days) : ICommand<BackfillStatus>;

public sealed record GetBackfillStatusQuery(Guid CardId) : IQuery<BackfillStatus>;

public class BackfillCommandsHandler :
    ICommandHandler<StartBackfillCommand, BackfillStatus>,
    ICommandHandler<StartResyncCommand, BackfillStatus>,
    IQueryHandler<GetBackfillStatusQuery, BackfillStatus>
{
    private const int MaxMonths = 24;
    private const int MaxDays = 400;
    private readonly IBackfillQueue _queue;
    private readonly IClientTransactionRepository _transactions;

    public BackfillCommandsHandler(IBackfillQueue queue, IClientTransactionRepository transactions)
    {
        _queue = queue;
        _transactions = transactions;
    }

    public async Task<Result<BackfillStatus>> Handle(StartBackfillCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var earliest = await _transactions.GetEarliest(request.CardId, cancellationToken);
        var to = (earliest is null ? now : new DateTimeOffset(earliest.DateOccured)).ToUnixTimeSeconds() - 1;
        var floor = now.AddMonths(-Math.Clamp(request.Months, 1, MaxMonths)).ToUnixTimeSeconds();
        return await _queue.Enqueue(request.CardId, to, floor, cancellationToken);
    }

    public async Task<Result<BackfillStatus>> Handle(StartResyncCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var floor = now.AddDays(-Math.Clamp(request.Days, 1, MaxDays)).ToUnixTimeSeconds();
        return await _queue.Enqueue(request.CardId, now.ToUnixTimeSeconds(), floor, cancellationToken);
    }

    public async Task<Result<BackfillStatus>> Handle(GetBackfillStatusQuery request, CancellationToken cancellationToken) =>
        await _queue.Get(request.CardId, cancellationToken);
}
