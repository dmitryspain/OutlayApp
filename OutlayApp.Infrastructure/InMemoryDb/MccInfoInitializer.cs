using System.Net.Http.Json;
using OutlayApp.Infrastructure.Models;

namespace OutlayApp.Infrastructure.InMemoryDb;

public static class MccInfoInitializer
{
    public static async Task AddMccs(OutlayInMemoryContext context, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        const string mccCodesPath = "https://raw.githubusercontent.com/Oleksios/Merchant-Category-Codes/main/Without%20groups/mcc-uk.json";
        var records = httpClient.GetFromJsonAsync<IEnumerable<MccInfo>>(mccCodesPath, cancellationToken).Result;
        await context.AddRangeAsync(records!, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}