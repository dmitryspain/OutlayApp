using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;

public record GetClientTransactionsQuery(Guid ClientCardId, long? DateFrom, long? DateTo) 
    : IQuery<List<ClientTransactionDto>>;
