using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Transactions;

namespace OutlayApp.Application.Clients.Queries.GetClientTransactions;

public record GetClientTransactionsQuery(Guid ClientCardId, long? DateFrom, long? DateTo) 
    : IQuery<List<ClientTransactionDto>>;
