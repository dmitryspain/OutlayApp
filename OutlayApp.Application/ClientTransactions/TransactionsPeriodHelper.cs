using OutlayApp.Application.Time;

namespace OutlayApp.Application.ClientTransactions;

public static class TransactionsPeriodHelper
{
    private const int DefaultDays = 30;

    /// <summary>The requested period in UTC; the last 30 days when a bound is missing.</summary>
    public static (DateTime From, DateTime To) Resolve(DateTime? dateFrom, DateTime? dateTo)
    {
        var to = dateTo is null ? DateTime.UtcNow : KyivTime.ToUtc(dateTo.Value);
        var from = dateFrom is null ? to.AddDays(-DefaultDays) : KyivTime.ToUtc(dateFrom.Value);
        return (from, to);
    }
}
