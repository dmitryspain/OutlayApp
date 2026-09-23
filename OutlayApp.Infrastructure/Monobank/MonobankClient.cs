using System.Net;
using System.Net.Http.Json;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Monobank;
using OutlayApp.Application.Transactions;

namespace OutlayApp.Infrastructure.Monobank;

/// <summary>The only place that talks to api.monobank.ua.</summary>
public sealed class MonobankClient : IMonobankClient
{
    private const string ClientInfoEndpoint = "client-info";
    private const string StatementEndpoint = "statement";

    private readonly HttpClient _http;
    private readonly MonobankRateLimiter _limiter;

    public MonobankClient(IHttpClientFactory factory, MonobankRateLimiter limiter)
    {
        _http = factory.CreateClient(MonobankConstants.HttpClient);
        _limiter = limiter;
    }

    public async Task<MonobankClientInfo> GetClientInfo(string token, CancellationToken cancellationToken)
    {
        _limiter.Acquire(token, ClientInfoEndpoint);
        using var response = await Send(HttpMethod.Get, "/personal/client-info", token, null, ClientInfoEndpoint,
            cancellationToken);
        return await response.Content.ReadFromJsonAsync<MonobankClientInfo>(cancellationToken)
               ?? throw new MonobankException("Empty client-info response");
    }

    public async Task<List<MonobankTransaction>> GetStatement(string token, string account, long from, long to,
        CancellationToken cancellationToken)
    {
        _limiter.Acquire(token, StatementEndpoint);
        using var response = await Send(HttpMethod.Get, $"/personal/statement/{account}/{from}/{to}", token, null,
            StatementEndpoint, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<MonobankTransaction>>(cancellationToken)
               ?? new List<MonobankTransaction>();
    }

    public async Task SetWebhook(string token, string url, CancellationToken cancellationToken)
    {
        using var _ = await Send(HttpMethod.Post, "/personal/webhook", token, JsonContent.Create(new { webHookUrl = url }),
            "webhook", cancellationToken);
    }

    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, string token, HttpContent? content,
        string endpoint, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        request.Headers.Add(MonobankConstants.TokenHeader, token);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new MonobankException($"Monobank unreachable: {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            throw new MonobankException($"Monobank timed out: {ex.Message}");
        }

        if (response.IsSuccessStatusCode)
            return response;

        using (response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.TooManyRequests:
                    _limiter.Penalise(token, endpoint);
                    throw new MonobankRateLimitException(MonobankConstants.RequestInterval);
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    throw new MonobankUnauthorizedException();
                default:
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new MonobankException($"Monobank {(int)response.StatusCode}: {body}", (int)response.StatusCode);
            }
        }
    }
}
