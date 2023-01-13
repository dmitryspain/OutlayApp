using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Commands;

public sealed record FetchLatestTransactionsCommand(Guid ClientId, string ExternalCardId) : ICommand;