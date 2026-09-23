using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;

public record GetClientTransactionsByDescriptionQuery(Guid ClientCardId, string Description, DateTime? DateFrom, DateTime? DateTo) 
    : IQuery<List<ClientTransactionByDescriptionResponse>>;
