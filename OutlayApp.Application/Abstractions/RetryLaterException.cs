namespace OutlayApp.Application.Abstractions;

/// <summary>A background event could not be handled yet (e.g. the bank's rate limit); the outbox keeps it for later.</summary>
public sealed class RetryLaterException : Exception
{
    public RetryLaterException(string message) : base(message)
    {
    }
}
