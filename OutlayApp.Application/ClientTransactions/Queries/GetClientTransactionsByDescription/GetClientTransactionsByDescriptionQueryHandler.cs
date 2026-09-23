using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Time;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;

public class GetClientTransactionsByDescriptionQueryHandler : IQueryHandler<GetClientTransactionsByDescriptionQuery,
    List<ClientTransactionByDescriptionResponse>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;

    public GetClientTransactionsByDescriptionQueryHandler(IClientTransactionRepository clientTransactionRepository)
    {
        _clientTransactionRepository = clientTransactionRepository;
    }

    public async Task<Result<List<ClientTransactionByDescriptionResponse>>> Handle(GetClientTransactionsByDescriptionQuery request,
        CancellationToken cancellationToken)
    {
        var (from, to) = TransactionsPeriodHelper.Resolve(request.DateFrom, request.DateTo);
        var transactions = await _clientTransactionRepository
            .GetByDescription(request.ClientCardId, request.Description, from, to, cancellationToken);

        return transactions.Select(x => new ClientTransactionByDescriptionResponse
        {
            Name = x.Description,
            DateOccured = $"{KyivTime.FromUtc(x.DateOccured):g}",
            Amount = x.Amount,
        }).ToList();
    }
}
