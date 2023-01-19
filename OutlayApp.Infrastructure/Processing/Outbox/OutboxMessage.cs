namespace OutlayApp.Infrastructure.Processing.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long OccurredOnUtc { get; set; }
    public long? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}