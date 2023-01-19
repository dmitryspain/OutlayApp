using System.Globalization;

namespace OutlayApp.Application.Configuration.Extensions;

public static class MoneyExtensions
{
    public static decimal ToDecimal(this int combined)
    {
        var fraction = combined.ToString(CultureInfo.InvariantCulture)[^2..];
        var beforeFraction = combined.ToString(CultureInfo.InvariantCulture)[..^2];
        var separated = $"{beforeFraction}.{fraction}";
        return Convert.ToDecimal(separated, new CultureInfo("en-US"));
    }
}