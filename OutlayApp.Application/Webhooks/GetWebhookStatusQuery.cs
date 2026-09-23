using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Webhooks;

public sealed record GetWebhookStatusQuery(string ClientToken) : IQuery<WebhookStatus>;

/// <param name="Configured">the server has a public URL to offer Monobank</param>
/// <param name="Enabled">this client's webhook points at the current URL</param>
public sealed record WebhookStatus(bool Configured, bool Enabled);
