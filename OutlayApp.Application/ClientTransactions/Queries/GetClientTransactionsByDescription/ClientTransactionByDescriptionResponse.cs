namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;

public class ClientTransactionByDescriptionResponse
{
    public string Name { get; set; }
    public string DateOccured { get; set; }
    public decimal Amount { get; set; }
}