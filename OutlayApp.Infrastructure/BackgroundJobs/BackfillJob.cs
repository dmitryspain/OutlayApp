namespace OutlayApp.Infrastructure.BackgroundJobs;

/// <summary>A card's history job: read the statement backwards from <see cref="CursorTo"/> down to <see cref="Floor"/>.</summary>
public sealed class BackfillJob
{
    public Guid CardId { get; set; }
    public string State { get; set; } = string.Empty;
    /// <summary>unix seconds: where the job started, where the next window ends, and where it stops</summary>
    public long Start { get; set; }
    public long CursorTo { get; set; }
    public long Floor { get; set; }
    public int Imported { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
    public DateTime NextRunAtUtc { get; set; }
    /// <summary>an instance working on it holds it until then (a crashed one lets go by itself)</summary>
    public DateTime? LockedUntilUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
