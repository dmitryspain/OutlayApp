using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Time;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsWeekly;

public class GetClientTransactionsWeeklyQueryHandler : IQueryHandler<GetClientTransactionsWeeklyQuery,
    List<ClientTransactionsWeeklyResponse>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly ITransactionEnricher _enricher;

    public GetClientTransactionsWeeklyQueryHandler(IClientTransactionRepository clientTransactionRepository,
        ITransactionEnricher enricher)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _enricher = enricher;
    }

    public async Task<Result<List<ClientTransactionsWeeklyResponse>>> Handle(GetClientTransactionsWeeklyQuery request,
        CancellationToken cancellationToken)
    {
        const int daysInWeek = 7;
        // the week is a Kyiv calendar week (Sunday first, as before)
        var today = KyivTime.Now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek).AddDays(-daysInWeek * request.SkipWeeks);
        var from = KyivTime.ToUtc(weekStart);
        var to = KyivTime.ToUtc(weekStart.AddDays(daysInWeek));

        var transactions = await _clientTransactionRepository.GetByPeriod(request.ClientCardId, from, to, cancellationToken);
        var dtos = await _enricher.ToDtos(transactions, cancellationToken);

        return dtos.GroupBy(x => KyivTime.FromUtc(x.DateOccured).DayOfWeek)
            .Select(x => new ClientTransactionsWeeklyResponse
            {
                DayOfWeek = x.Key,
                Amount = x.Sum(dto => dto.Amount),
                Transactions = x.ToList(),
            })
            .OrderBy(x => x.DayOfWeek)
            .ToList();
    }
}
