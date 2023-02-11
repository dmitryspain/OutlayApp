using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;

namespace OutlayApp.Application.ClientTransactions;

public class ClientTransactionsWeeklyResponse
{
    public DayOfWeek DayOfWeek { get; set; }
    public decimal Amount { get; set; }
    public List<ClientTransactionDto> Transactions { get; set; } = new();
}