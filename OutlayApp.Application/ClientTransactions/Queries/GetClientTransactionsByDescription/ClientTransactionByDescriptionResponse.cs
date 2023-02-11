namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;

public class ClientTransactionByDescriptionResponse
{
    public string Name { get; set; }
    public DateTime DateOccured { get; set; }
    public decimal Amount { get; set; }
}