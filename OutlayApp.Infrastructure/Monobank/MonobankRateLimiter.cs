using System.Collections.Concurrent;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Monobank;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Infrastructure.Monobank;

/// <summary>
/// Monobank allows each personal endpoint once a minute per token. Checking locally first means a request
/// we already know would get 429 is refused at once (with the wait), instead of costing a call.
/// </summary>
public sealed class MonobankRateLimiter
{
    private readonly ConcurrentDictionary<string, DateTime> _next = new();
    private readonly TimeProvider _time;

    public MonobankRateLimiter(TimeProvider time)
    {
        _time = time;
    }

    /// <summary>Takes the slot or throws <see cref="MonobankRateLimitException"/> with the time left.</summary>
    public void Acquire(string token, string endpoint)
    {
        var key = $"{TokenHash.Of(token)}:{endpoint}";
        var now = _time.GetUtcNow().UtcDateTime;
        while (true)
        {
            if (_next.TryGetValue(key, out var next))
            {
                if (next > now)
                    throw new MonobankRateLimitException(next - now);
                if (_next.TryUpdate(key, now + MonobankConstants.RequestInterval, next))
                    return;
            }
            else if (_next.TryAdd(key, now + MonobankConstants.RequestInterval))
            {
                return;
            }
        }
    }

    /// <summary>The bank answered 429 anyway (another instance, or the app restarted): wait a full interval.</summary>
    public void Penalise(string token, string endpoint) =>
        _next[$"{TokenHash.Of(token)}:{endpoint}"] = _time.GetUtcNow().UtcDateTime + MonobankConstants.RequestInterval;
}
