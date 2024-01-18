using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Clients.Commands;

public sealed record RegisterClientCommand(string ClientToken) : ICommand<Guid>;