using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Transactions.Commands;

public sealed record FetchLatestTransactionsCommand(string CardId) : ICommand;