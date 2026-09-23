namespace OutlayApp.Application.Time;

/// <summary>
/// Everything is stored in UTC; Kyiv time is only for calendar logic ("this week") and for reading
/// dates that arrive without an offset (older clients send local Kyiv time).
/// </summary>
public static class KyivTime
{
    public static readonly TimeZoneInfo Zone = Find();

    private static TimeZoneInfo Find()
    {
        foreach (var id in new[] { "Europe/Kyiv", "Europe/Kiev", "FLE Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
                return zone;
        }
        throw new InvalidOperationException("Kyiv time zone is not available on this system");
    }

    /// <summary>Normalises a date from a request to UTC: explicit offsets are honoured, bare dates mean Kyiv time.</summary>
    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => TimeZoneInfo.ConvertTimeToUtc(value, Zone),
    };

    public static DateTime FromUtc(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

    public static DateTime Now => FromUtc(DateTime.UtcNow);
}
