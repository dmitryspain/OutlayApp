using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Application.Clients.Queries.GetClientTransactions;

public class ClientTransactionDto
{
    public Guid Id { get; set; }
    public DateTime DateOccured { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
}