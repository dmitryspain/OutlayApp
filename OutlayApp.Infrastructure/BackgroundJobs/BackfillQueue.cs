using Microsoft.EntityFrameworkCore;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Live;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.BackgroundJobs;

public sealed class BackfillQueue : IBackfillQueue
{
    private readonly OutlayContext _db;
    private readonly ILiveEvents _live;

    public BackfillQueue(OutlayContext db, ILiveEvents live)
    {
        _db = db;
        _live = live;
    }

    public async Task<BackfillStatus> Enqueue(Guid cardId, long to, long floor, CancellationToken cancellationToken)
    {
        var job = await _db.BackfillJobs.FirstOrDefaultAsync(x => x.CardId == cardId, cancellationToken);
        if (job?.State == BackfillStates.Running)
            return ToStatus(job);

        var now = DateTime.UtcNow;
        job ??= _db.BackfillJobs.Add(new BackfillJob { CardId = cardId }).Entity;
        job.State = to > floor ? BackfillStates.Running : BackfillStates.Done;
        job.Start = to;
        job.CursorTo = to;
        job.Floor = floor;
        job.Imported = 0;
        job.Attempts = 0;
        job.Error = null;
        job.NextRunAtUtc = now;
        job.LockedUntilUtc = null;
        job.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);

        var status = ToStatus(job);
        await _live.Publish(cardId, new LiveEvent(LiveEventTypes.Backfill, status));
        return status;
    }

    public async Task<BackfillStatus> Get(Guid cardId, CancellationToken cancellationToken)
    {
        var job = await _db.BackfillJobs.AsNoTracking().FirstOrDefaultAsync(x => x.CardId == cardId, cancellationToken);
        return job is null ? BackfillStatus.Idle(cardId) : ToStatus(job);
    }

    public static BackfillStatus ToStatus(BackfillJob job)
    {
        var done = job.State == BackfillStates.Done;
        var total = Math.Max(1, job.Start - job.Floor);
        var progress = done ? 1 : Math.Clamp((double)(job.Start - job.CursorTo) / total, 0, 1);
        var windowsLeft = (int)Math.Ceiling((double)Math.Max(0, job.CursorTo - job.Floor) / MonobankConstants.MaxStatementSeconds);
        return new BackfillStatus(job.CardId, job.State, progress, job.Imported,
            DateTimeOffset.FromUnixTimeSeconds(Math.Max(job.CursorTo, job.Floor) + 1).UtcDateTime,
            job.State == BackfillStates.Running ? windowsLeft * (int)MonobankConstants.RequestInterval.TotalSeconds : null,
            job.Error);
    }
}
