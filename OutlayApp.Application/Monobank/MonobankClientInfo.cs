namespace OutlayApp.Application.Monobank;

/// <summary>GET /personal/client-info.</summary>
public class MonobankClientInfo
{
    public string ClientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? WebHookUrl { get; set; }
    public string? Permissions { get; set; }
    public List<MonobankAccount> Accounts { get; set; } = new();
}

public class MonobankAccount
{
    public string Id { get; set; } = string.Empty;
    public string? SendId { get; set; }
    /// <summary>Minor units.</summary>
    public long Balance { get; set; }
    /// <summary>Minor units.</summary>
    public long CreditLimit { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CurrencyCode { get; set; }
    public string? CashbackType { get; set; }
    public string? Iban { get; set; }
    public List<string> MaskedPan { get; set; } = new();
}
