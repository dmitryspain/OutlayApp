namespace OutlayApp.Application.Transactions;

/// <summary>Monobank answered 429: a statement may be requested once per 60 seconds per token.</summary>
public sealed class MonobankRateLimitException : Exception
{
    public MonobankRateLimitException() : base("Monobank rate limit: one statement request per 60 seconds")
    {
    }
}
