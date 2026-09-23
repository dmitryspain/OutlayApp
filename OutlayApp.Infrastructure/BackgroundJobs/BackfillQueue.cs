using System.Collections.Concurrent;
using System.Threading.Channels;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.Live;

namespace OutlayApp.Infrastructure.BackgroundJobs;

public sealed class BackfillQueue : IBackfillQueue
{
    private const int MaxMonths = 24;
    private readonly Channel<(Guid CardId, int Months)> _jobs = Channel.CreateUnbounded<(Guid, int)>();
    private readonly ConcurrentDictionary<Guid, BackfillStatus> _status = new();
    private readonly ILiveEvents _liveEvents;

    public BackfillQueue(ILiveEvents liveEvents)
    {
        _liveEvents = liveEvents;
    }

    internal ChannelReader<(Guid CardId, int Months)> Jobs => _jobs.Reader;

    public BackfillStatus Enqueue(Guid cardId, int months)
    {
        var current = Get(cardId);
        if (current.State == BackfillStates.Running)
            return current;

        var status = new BackfillStatus(cardId, BackfillStates.Running, 0, 0, null, null);
        Report(status);
        _jobs.Writer.TryWrite((cardId, Math.Clamp(months, 1, MaxMonths)));
        return status;
    }

    public BackfillStatus Get(Guid cardId) =>
        _status.TryGetValue(cardId, out var status) ? status : BackfillStatus.Idle(cardId);

    internal void Report(BackfillStatus status)
    {
        _status[status.CardId] = status;
        _liveEvents.Publish(status.CardId, new LiveEvent(LiveEventTypes.Backfill, status));
    }
}
