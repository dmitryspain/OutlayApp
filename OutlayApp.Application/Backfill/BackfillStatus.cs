namespace OutlayApp.Application.Backfill;

public static class BackfillStates
{
    public const string Idle = "idle";
    public const string Running = "running";
    public const string Done = "done";
    public const string Failed = "failed";
}

/// <summary>Progress of loading (or re-reading) a card's history.</summary>
/// <param name="Progress">0..1</param>
/// <param name="OldestLoaded">how far back the job has got (UTC)</param>
/// <param name="EtaSeconds">rough time left (Monobank allows one statement request per minute)</param>
public sealed record BackfillStatus(Guid CardId, string State, double Progress, int Imported,
    DateTime? OldestLoaded, int? EtaSeconds, string? Error = null)
{
    public static BackfillStatus Idle(Guid cardId) => new(cardId, BackfillStates.Idle, 0, 0, null, null);
}

/// <summary>
/// Durable queue of history jobs (one per card), worked through one statement window per minute.
/// Survives restarts and can be shared by several server instances.
/// </summary>
public interface IBackfillQueue
{
    /// <summary>Reads the statement from <paramref name="to"/> back to <paramref name="floor"/> (unix seconds).
    /// A job already running for the card is kept as it is.</summary>
    Task<BackfillStatus> Enqueue(Guid cardId, long to, long floor, CancellationToken cancellationToken);

    Task<BackfillStatus> Get(Guid cardId, CancellationToken cancellationToken);
}
