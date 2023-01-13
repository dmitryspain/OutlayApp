using System.Net.Http.Json;
using AutoMapper;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Clients.Queries.GetClientTransactions;

public class GetClientTransactionsQueryHandler : IQueryHandler<GetClientTransactionsQuery, List<ClientTransactionDto>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly IMapper _mapper;
    private const int MaxDaysPeriod = 30;

    public GetClientTransactionsQueryHandler(IClientTransactionRepository clientTransactionRepository, IMapper mapper)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ClientTransactionDto>>> Handle(GetClientTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var dateFrom = DateTimeOffset.Now.AddDays(-MaxDaysPeriod).ToUnixTimeSeconds();
        var dateTo = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (request.DateFrom.HasValue)
            dateFrom = request.DateFrom.Value;

        if (request.DateTo.HasValue)
            dateTo = request.DateTo.Value;

        var transactions = await _clientTransactionRepository.GetByPeriod(request.ClientCardId, dateFrom, dateTo, cancellationToken);
        return _mapper.Map<List<ClientTransactionDto>>(transactions);
    }
}