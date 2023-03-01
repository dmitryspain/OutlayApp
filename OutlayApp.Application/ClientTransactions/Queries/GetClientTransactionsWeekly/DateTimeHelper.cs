namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsWeekly;

public static class DateTimeHelper
{
    public static int ToStandardDayOfWeek(DayOfWeek dayOfWeek)
    {
        if (dayOfWeek == DayOfWeek.Sunday)
            return 7;

        return (int)dayOfWeek - 1;
    }
}