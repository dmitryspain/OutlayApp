using AutoMapper;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;

public class GetClientTransactionsQueryHandler : IQueryHandler<GetClientTransactionsQuery, List<ClientTransactionDto>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly IMapper _mapper;

    public GetClientTransactionsQueryHandler(IClientTransactionRepository clientTransactionRepository, IMapper mapper)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ClientTransactionDto>>> Handle(GetClientTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var (dateFrom, dateTo) = TransactionsPeriodHelper
            .GetMonobankTransactionsPeriod(request.DateFrom, request.DateTo);

        var transactions = await _clientTransactionRepository.GetByPeriod(request.ClientCardId, dateFrom, dateTo, cancellationToken);
        return _mapper.Map<List<ClientTransactionDto>>(transactions);
    }
}