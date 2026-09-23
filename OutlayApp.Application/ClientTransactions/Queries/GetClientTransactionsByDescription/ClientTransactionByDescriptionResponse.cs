namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;

public class ClientTransactionByDescriptionResponse
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Kyiv time, formatted.</summary>
    public string DateOccured { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
