using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;

public record GetClientTransactionsGroupedQuery(Guid ClientCardId, DateTime? DateFrom, DateTime? DateTo) 
    : IQuery<List<ClientTransactionsGroupedResponse>>;
