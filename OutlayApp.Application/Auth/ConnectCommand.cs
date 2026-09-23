using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Auth;

/// <summary>The Monobank token is sent once; the browser gets a session token instead.</summary>
public sealed record ConnectCommand(string MonobankToken) : ICommand<ConnectResult>;

/// <param name="CardId">the card to show first</param>
public sealed record ConnectResult(string AccessToken, Guid? CardId, string Name);
