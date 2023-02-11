using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;

public record GetClientTransactionsQuery(Guid ClientCardId, DateTime? DateFrom, DateTime? DateTo) 
    : IQuery<List<ClientTransactionDto>>;
