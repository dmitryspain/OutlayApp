using OutlayApp.Application.Configuration.Monobank;

namespace OutlayApp.Application.Webhooks;

public static class WebhookUrl
{
    public const string Route = "api/webhooks/monobank";

    /// <summary>The public webhook address, or null when the server has no public URL / secret configured.</summary>
    public static string? Build(MonobankSettings settings) =>
        string.IsNullOrWhiteSpace(settings.WebhookBaseUrl) || string.IsNullOrWhiteSpace(settings.WebhookSecret)
            ? null
            : $"{settings.WebhookBaseUrl.TrimEnd('/')}/{Route}/{settings.WebhookSecret}";
}
