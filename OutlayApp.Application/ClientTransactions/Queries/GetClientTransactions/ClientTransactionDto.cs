namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;

public class ClientTransactionDto
{
    public Guid Id { get; set; }
    /// <summary>UTC.</summary>
    public DateTime DateOccured { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? CounterName { get; set; }
    public string? Comment { get; set; }
    public decimal Cashback { get; set; }
    public bool Hold { get; set; }
}
