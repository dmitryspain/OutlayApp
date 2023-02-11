namespace OutlayApp.Application.ClientTransactions;

public static class TransactionsPeriodHelper
{
    public static (DateTime, DateTime) GetMonobankTransactionsPeriod(DateTime? dateFrom, DateTime? dateTo)
    {
        const int maxDaysPeriod = 30;
        var defaultDateFrom = DateTime.Now.AddDays(-maxDaysPeriod);
        var defaultDateTo = DateTime.Now;

        dateFrom ??= defaultDateFrom;
        dateTo ??= defaultDateTo;
        
        return (dateFrom.Value, dateTo.Value);
    }
}