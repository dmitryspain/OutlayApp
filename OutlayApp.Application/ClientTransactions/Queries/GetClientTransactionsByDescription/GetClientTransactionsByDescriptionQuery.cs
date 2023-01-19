using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;

public record GetClientTransactionsByDescriptionQuery(Guid ClientCardId, string Description) 
    : IQuery<List<ClientTransactionByDescriptionResponse>>;
