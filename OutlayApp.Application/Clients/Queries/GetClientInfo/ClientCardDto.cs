namespace OutlayApp.Application.Clients.Queries.GetClientInfo;

public class ClientCardDto
{
    public Guid Id { get; set; }
    /// <summary>Whole currency units.</summary>
    public decimal Balance { get; set; }
    /// <summary>Whole currency units.</summary>
    public decimal CreditLimit { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CurrencyCode { get; set; }
    public string? MaskedCardNumber { get; set; }
    public string? Iban { get; set; }
}
