namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public class ClientCardDto
{
    public string Id { get; set; }
    public int Balance { get; set; }
    public int CurrencyCode { get; set; }
    public string Type { get; set; }
    public string MaskedCardNumber { get; set; }
}