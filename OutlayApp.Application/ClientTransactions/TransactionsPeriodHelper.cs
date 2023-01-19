namespace OutlayApp.Application.ClientTransactions;

public static class TransactionsPeriodHelper
{
    public static (long, long) GetMonobankTransactionsPeriod(long? dateFrom, long? dateTo)
    {
        const int maxDaysPeriod = 30;
        var defaultDateFrom = DateTimeOffset.Now.AddDays(-maxDaysPeriod).ToUnixTimeSeconds();
        var defaultDateTo = DateTimeOffset.Now.ToUnixTimeSeconds();

        dateFrom ??= defaultDateFrom;
        dateTo ??= defaultDateTo;
        
        return (dateFrom.Value, dateTo.Value);
    }
}