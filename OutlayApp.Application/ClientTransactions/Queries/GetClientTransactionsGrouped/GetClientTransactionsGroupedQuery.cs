using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;

public record GetClientTransactionsGroupedQuery(Guid ClientCardId, long? DateFrom, long? DateTo) 
    : IQuery<List<ClientTransactionsGroupedResponse>>;
