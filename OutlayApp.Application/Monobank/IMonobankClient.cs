using OutlayApp.Application.Transactions;

namespace OutlayApp.Application.Monobank;

/// <summary>
/// The Monobank personal API. Throws <see cref="MonobankRateLimitException"/> (also before calling, when the
/// token used that endpoint less than a minute ago), <see cref="MonobankUnauthorizedException"/> and
/// <see cref="MonobankException"/>.
/// </summary>
public interface IMonobankClient
{
    Task<MonobankClientInfo> GetClientInfo(string token, CancellationToken cancellationToken);

    /// <summary>One statement page (≤ 500 items, newest first) for [from, to], unix seconds.</summary>
    Task<List<MonobankTransaction>> GetStatement(string token, string account, long from, long to,
        CancellationToken cancellationToken);

    Task SetWebhook(string token, string url, CancellationToken cancellationToken);
}
