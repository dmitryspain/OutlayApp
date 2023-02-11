using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OutlayApp.Application.Configuration.Google;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Services;

public class GoogleImageSearchService : IGoogleImageSearchService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _client;

    public GoogleImageSearchService(IHttpClientFactory factory, IConfiguration configuration)
    {
        _configuration = configuration;
        _client = factory.CreateClient();
    }

    public async Task<string> GetCompanyLogo(string logoName, int mcc, CancellationToken cancellationToken)
    {
        if (mcc == 4829) //5411, 5950, 4814, 5912, 5499, 5812, 6536, 6538
            return string.Empty;
        
        var key = _configuration[GoogleConstants.Key];
        var engineId = _configuration[GoogleConstants.EngineId];

        var searchUrl = $"https://www.googleapis.com/customsearch/v1?key={key}&cx={engineId}&q={logoName}";
        using var client = new HttpClient();
        var message = await client.GetAsync(searchUrl, cancellationToken);

        var json = await message.Content.ReadAsStringAsync(cancellationToken);
        var jObject = JObject.Parse(json);
        try
        {
            var logoSource = jObject["items"]!
                .Select(x => x["pagemap"])
                .Select(s => s!["cse_image"])
                .FirstOrDefault()!
                .Select(x => x["src"])
                .FirstOrDefault()!
                .Value<string>()!;
            
            return logoSource;
        }
        catch (Exception e)
        {
            return string.Empty;
        }
    }
}