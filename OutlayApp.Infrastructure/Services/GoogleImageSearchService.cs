using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OutlayApp.Application.Configuration.Google;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Services;

public class GoogleImageSearchService : IGoogleImageSearchService
{
    private readonly IConfiguration _configuration;

    public GoogleImageSearchService(IConfiguration configuration, IMemoryCache cache)
    {
        _configuration = configuration;
    }

    public async Task<string> GetCompanyLogo(string logoName, CancellationToken cancellationToken)
    {
        //todo maybe could be optimized, use array of logo names and check only once
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