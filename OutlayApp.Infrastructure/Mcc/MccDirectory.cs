using System.Text.Json;
using System.Text.Json.Serialization;

namespace OutlayApp.Infrastructure.Mcc;

/// <summary>Merchant category codes → short Ukrainian names. Shipped with the app (it used to be downloaded at every start).</summary>
public sealed class MccDirectory
{
    private readonly Dictionary<int, string> _names;

    public MccDirectory()
    {
        using var stream = typeof(MccDirectory).Assembly.GetManifestResourceStream("mcc-uk.json")
                           ?? throw new InvalidOperationException("mcc-uk.json is not embedded");
        var records = JsonSerializer.Deserialize<List<Record>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        }) ?? new List<Record>();
        _names = records.GroupBy(r => r.Mcc).ToDictionary(g => g.Key, g => g.First().ShortDescription);
    }

    public string NameOf(int mcc) => _names.GetValueOrDefault(mcc, string.Empty);

    private sealed record Record(int Mcc, string ShortDescription);
}
