using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Clients.Commands;

public sealed record RegisterClientCommand(string ClientToken) : ICommand<Guid>;