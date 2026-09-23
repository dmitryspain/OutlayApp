using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;

public class GetClientTransactionsGroupedQueryHandler : IQueryHandler<GetClientTransactionsGroupedQuery,
    List<ClientTransactionsGroupedResponse>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly ITransactionEnricher _enricher;

    public GetClientTransactionsGroupedQueryHandler(IClientTransactionRepository clientTransactionRepository,
        ITransactionEnricher enricher)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _enricher = enricher;
    }

    public async Task<Result<List<ClientTransactionsGroupedResponse>>> Handle(GetClientTransactionsGroupedQuery request,
        CancellationToken cancellationToken)
    {
        var (from, to) = TransactionsPeriodHelper.Resolve(request.DateFrom, request.DateTo);
        var transactions = await _clientTransactionRepository.GetByPeriod(request.ClientCardId, from, to, cancellationToken);

        var groups = transactions.GroupBy(x => x.Description).ToList();
        var logos = await _enricher.LogosFor(groups.Select(g => ITransactionEnricher.LogoName(g.Key)).Distinct().ToList(),
            cancellationToken);

        return groups
            .Select(g => new ClientTransactionsGroupedResponse
            {
                Name = g.Key,
                Amount = g.Sum(s => s.Amount),
                Category = _enricher.CategoryOf(g.First().Mcc),
                Icon = logos.GetValueOrDefault(ITransactionEnricher.LogoName(g.Key), string.Empty),
            })
            .OrderBy(x => x.Amount)
            .ToList();
    }
}
