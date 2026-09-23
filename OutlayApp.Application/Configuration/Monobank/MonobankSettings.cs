namespace OutlayApp.Application.Configuration.Monobank;

public class MonobankSettings
{
    public string BaseUrl { get; set; }

    /// <summary>Public https address of this API (e.g. a tunnel in development). Empty = webhooks are off.</summary>
    public string? WebhookBaseUrl { get; set; }

    /// <summary>Secret path segment, so only Monobank (who was told the URL) can post statement items.</summary>
    public string? WebhookSecret { get; set; }
}
