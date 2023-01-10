using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Clients.Command;

public sealed record RegisterClientCommand(string ClientToken) : ICommand;