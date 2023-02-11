using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsWeekly;

public record GetClientTransactionsWeeklyQuery(Guid ClientCardId, int SkipWeeks = 0) : IQuery<List<ClientTransactionsWeeklyResponse>>;
