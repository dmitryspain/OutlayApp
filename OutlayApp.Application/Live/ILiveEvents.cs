using System.Threading.Channels;

namespace OutlayApp.Application.Live;

/// <summary>Something the open browser tabs of a card should hear about right away.</summary>
public sealed record LiveEvent(string Type, object Data);

public static class LiveEventTypes
{
    public const string Transaction = "transaction";
    public const string Backfill = "backfill";
}

/// <summary>An open listener; dispose it when the connection goes away.</summary>
public interface ILiveSubscription : IDisposable
{
    ChannelReader<LiveEvent> Reader { get; }
}

/// <summary>In-process fan-out of <see cref="LiveEvent"/>s to the listeners of a card (streamed to the UI as SSE).</summary>
public interface ILiveEvents
{
    void Publish(Guid cardId, LiveEvent liveEvent);
    ILiveSubscription Subscribe(Guid cardId);
}
