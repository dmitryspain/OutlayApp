using System.Collections.Concurrent;
using System.Threading.Channels;
using OutlayApp.Application.Live;

namespace OutlayApp.Infrastructure.Live;

/// <summary>Single-process pub/sub. Each listener gets its own small channel; a slow one only loses its oldest events.</summary>
public sealed class LiveEvents : ILiveEvents
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Subscription, byte>> _listeners = new();

    public void Publish(Guid cardId, LiveEvent liveEvent)
    {
        if (!_listeners.TryGetValue(cardId, out var subs))
            return;
        foreach (var sub in subs.Keys)
            sub.Channel.Writer.TryWrite(liveEvent);
    }

    public ILiveSubscription Subscribe(Guid cardId)
    {
        var sub = new Subscription(this, cardId);
        _listeners.GetOrAdd(cardId, _ => new ConcurrentDictionary<Subscription, byte>()).TryAdd(sub, 0);
        return sub;
    }

    private void Remove(Subscription sub)
    {
        if (_listeners.TryGetValue(sub.CardId, out var subs))
            subs.TryRemove(sub, out _);
    }

    private sealed class Subscription : ILiveSubscription
    {
        private readonly LiveEvents _owner;

        public Subscription(LiveEvents owner, Guid cardId)
        {
            _owner = owner;
            CardId = cardId;
        }

        public Guid CardId { get; }

        public Channel<LiveEvent> Channel { get; } = System.Threading.Channels.Channel.CreateBounded<LiveEvent>(
            new BoundedChannelOptions(64) { FullMode = BoundedChannelFullMode.DropOldest });

        public ChannelReader<LiveEvent> Reader => Channel.Reader;

        public void Dispose()
        {
            _owner.Remove(this);
            Channel.Writer.TryComplete();
        }
    }
}
