using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Webhooks;

/// <summary>Asks Monobank to push this client's new transactions to our webhook. Returns the registered URL.</summary>
public sealed record RegisterWebhookCommand(string ClientToken) : ICommand<string>;
