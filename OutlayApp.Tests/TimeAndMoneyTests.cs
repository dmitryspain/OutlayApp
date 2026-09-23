using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.Configuration.Extensions;
using OutlayApp.Application.Time;

namespace OutlayApp.Tests;

public class TimeAndMoneyTests
{
    [Fact]
    public void Bare_dates_are_Kyiv_time()
    {
        // 23 Sept 2026 is summer time, UTC+3
        var utc = KyivTime.ToUtc(new DateTime(2026, 9, 23, 16, 9, 53, DateTimeKind.Unspecified));
        Assert.Equal(new DateTime(2026, 9, 23, 13, 9, 53, DateTimeKind.Utc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public void Winter_dates_use_the_winter_offset()
    {
        var utc = KyivTime.ToUtc(new DateTime(2026, 1, 15, 12, 0, 0));
        Assert.Equal(new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc), utc);
    }

    [Fact]
    public void Utc_dates_stay_as_they_are()
    {
        var value = new DateTime(2026, 9, 23, 13, 0, 0, DateTimeKind.Utc);
        Assert.Equal(value, KyivTime.ToUtc(value));
    }

    [Fact]
    public void Kyiv_round_trip()
    {
        var utc = new DateTime(2026, 9, 23, 21, 30, 0, DateTimeKind.Utc);
        Assert.Equal(new DateTime(2026, 9, 24, 0, 30, 0), KyivTime.FromUtc(utc));
    }

    [Fact]
    public void Missing_period_is_the_last_30_days_in_utc()
    {
        var (from, to) = TransactionsPeriodHelper.Resolve(null, null);
        Assert.Equal(DateTimeKind.Utc, to.Kind);
        Assert.Equal(30, (to - from).TotalDays, 3);
    }

    [Theory]
    [InlineData(12345, 123.45)]
    [InlineData(-5, -0.05)]
    [InlineData(99, 0.99)]
    [InlineData(0, 0)]
    public void Minor_units_become_hryvnias(long minor, double expected)
    {
        Assert.Equal((decimal)expected, minor.ToDecimal());
    }
}
