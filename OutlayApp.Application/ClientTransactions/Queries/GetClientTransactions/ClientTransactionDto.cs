namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;

public class ClientTransactionDto
{
    public Guid Id { get; set; }
    public DateTime DateOccured { get;  set; }
    public string Description { get;  set; }
    public string Category { get;  set; }
    public string Icon { get;  set; }
    public decimal Amount { get;  set; }
    public decimal BalanceAfter { get;  set; }
}