using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Transactions;

namespace OutlayApp.Application.Webhooks;

/// <param name="Account">Monobank account id (our card's ExternalCardId).</param>
public sealed record ProcessStatementItemCommand(string Account, MonobankTransaction Item) : ICommand;
