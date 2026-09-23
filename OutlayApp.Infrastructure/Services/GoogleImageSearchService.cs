using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OutlayApp.Application.Configuration.Google;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Services;

public class GoogleImageSearchService : IGoogleImageSearchService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<GoogleImageSearchService> _logger;

    public GoogleImageSearchService(IConfiguration configuration, IHttpClientFactory httpFactory,
        ILogger<GoogleImageSearchService> logger)
    {
        _configuration = configuration;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<string?> GetCompanyLogo(string logoName, CancellationToken cancellationToken)
    {
        var key = _configuration[GoogleConstants.Key];
        var engineId = _configuration[GoogleConstants.EngineId];
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(engineId))
            return null;

        var searchUrl = "https://www.googleapis.com/customsearch/v1" +
                        $"?key={Uri.EscapeDataString(key)}&cx={Uri.EscapeDataString(engineId)}&q={Uri.EscapeDataString(logoName)}";
        JObject json;
        try
        {
            using var response = await _httpFactory.CreateClient().GetAsync(searchUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                // quota or key problems are not "this merchant has no logo"
                _logger.LogWarning("Logo search failed: {Status}", (int)response.StatusCode);
                return null;
            }
            json = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or Newtonsoft.Json.JsonException)
        {
            _logger.LogWarning(ex, "Logo search failed");
            return null;
        }

        var src = json["items"]?
            .Select(x => x["pagemap"]?["cse_image"]?.FirstOrDefault()?["src"]?.Value<string>())
            .FirstOrDefault(x => !string.IsNullOrEmpty(x));
        return src ?? string.Empty;
    }
}
