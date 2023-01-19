using System.Net.Http.Json;
using OutlayApp.Application;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Services;

public class BrandFetchService : IBrandFetchService
{
    private readonly HttpClient _client;
    
    public BrandFetchService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    public async Task<string> GetCompanyLogo(string companyName, CancellationToken cancellationToken)
    {
        var searchUrl = $"https://api.brandfetch.io/v2/search/{companyName}";
        
        using var client = new HttpClient();
        var message = await client.GetAsync(searchUrl, cancellationToken);
        
        var brandFetchIcons = (await message.Content.ReadFromJsonAsync<IEnumerable<BrandFetchData>>
            (cancellationToken: cancellationToken))!.ToList();
        if (brandFetchIcons is null || !brandFetchIcons.Any())
            return string.Empty;
        
        // From the list of icons get first (best match)
        return brandFetchIcons.FirstOrDefault()!.Icon;
    }
}