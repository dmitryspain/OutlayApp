namespace OutlayApp.Application.Monobank;

public class MonobankException : Exception
{
    public MonobankException(string message, int? status = null) : base(message)
    {
        Status = status;
    }

    public int? Status { get; }
}

/// <summary>Monobank allows each personal endpoint once per 60 seconds per token.</summary>
public sealed class MonobankRateLimitException : MonobankException
{
    public MonobankRateLimitException(TimeSpan retryAfter)
        : base($"Monobank allows one request per minute; try again in {Math.Ceiling(retryAfter.TotalSeconds)} s", 429)
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan RetryAfter { get; }
}

/// <summary>The token was rejected (revoked or mistyped).</summary>
public sealed class MonobankUnauthorizedException : MonobankException
{
    public MonobankUnauthorizedException() : base("Monobank rejected the token", 401)
    {
    }
}
