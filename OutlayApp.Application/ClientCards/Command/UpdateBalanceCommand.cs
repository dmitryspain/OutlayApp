using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientCards.Command;

/// <summary>Refreshes balances (and new accounts) of a client from /personal/client-info.</summary>
public record UpdateBalanceCommand(Guid ClientId) : ICommand;
