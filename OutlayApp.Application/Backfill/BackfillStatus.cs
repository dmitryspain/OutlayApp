namespace OutlayApp.Application.Backfill;

public static class BackfillStates
{
    public const string Idle = "idle";
    public const string Running = "running";
    public const string Done = "done";
    public const string Failed = "failed";
}

/// <summary>Progress of loading a card's older history.</summary>
/// <param name="Progress">0..1</param>
/// <param name="OldestLoaded">the oldest point the stored history now reaches</param>
/// <param name="EtaSeconds">rough time left (Monobank allows one statement request per minute)</param>
public sealed record BackfillStatus(Guid CardId, string State, double Progress, int Imported,
    DateTime? OldestLoaded, int? EtaSeconds, string? Error = null)
{
    public static BackfillStatus Idle(Guid cardId) => new(cardId, BackfillStates.Idle, 0, 0, null, null);
}

/// <summary>Queue of backfill jobs, drained by a background worker one request per minute.</summary>
public interface IBackfillQueue
{
    /// <summary>Starts (or keeps running) loading history back to <paramref name="months"/> months ago.</summary>
    BackfillStatus Enqueue(Guid cardId, int months);
    BackfillStatus Get(Guid cardId);
}
