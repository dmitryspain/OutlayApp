namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public class ClientCardDto
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public string Type { get; set; }
    public int CurrencyCode { get; set; }
    public string maskedCardNumber { get; set; }
}