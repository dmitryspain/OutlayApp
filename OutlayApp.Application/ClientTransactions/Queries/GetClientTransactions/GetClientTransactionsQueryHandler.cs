using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;

public class GetClientTransactionsQueryHandler : IQueryHandler<GetClientTransactionsQuery, List<ClientTransactionDto>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly ITransactionEnricher _enricher;

    public GetClientTransactionsQueryHandler(IClientTransactionRepository clientTransactionRepository,
        ITransactionEnricher enricher)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _enricher = enricher;
    }

    public async Task<Result<List<ClientTransactionDto>>> Handle(GetClientTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var (from, to) = TransactionsPeriodHelper.Resolve(request.DateFrom, request.DateTo);
        var transactions = await _clientTransactionRepository.GetByPeriod(request.ClientCardId, from, to, cancellationToken);
        return await _enricher.ToDtos(transactions, cancellationToken);
    }
}
