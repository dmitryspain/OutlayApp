namespace OutlayApp.Application.Configuration.Extensions;

public static class MoneyExtensions
{
    /// <summary>Monobank sends money in minor units (kopiykas).</summary>
    public static decimal ToDecimal(this long minor) => minor / 100m;

    public static decimal ToDecimal(this int minor) => minor / 100m;
}
