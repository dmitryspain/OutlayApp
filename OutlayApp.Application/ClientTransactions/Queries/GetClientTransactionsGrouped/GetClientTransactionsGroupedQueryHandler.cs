using AutoMapper;
using MediatR;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.LogoReferences;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;

public class GetClientTransactionsGroupedQueryHandler : IQueryHandler<GetClientTransactionsGroupedQuery,
    List<ClientTransactionsGroupedResponse>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public GetClientTransactionsGroupedQueryHandler(IClientTransactionRepository clientTransactionRepository,
        IMapper mapper, ISender sender)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _mapper = mapper;
        _sender = sender;
    }

    public async Task<Result<List<ClientTransactionsGroupedResponse>>> Handle(GetClientTransactionsGroupedQuery request,
        CancellationToken cancellationToken)
    {
        var (dateFrom, dateTo) = TransactionsPeriodHelper
            .GetMonobankTransactionsPeriod(request.DateFrom, request.DateTo);

        var transactions = await _clientTransactionRepository
            .GetByPeriod(request.ClientCardId, dateFrom, dateTo, cancellationToken);

        var grouped = transactions!.GroupBy(x => x.Description)
            .Select(x => new GroupedTransaction
            {
                Name = x.Key,
                Amount = x.Sum(s => s.Amount),
                Mcc = x.FirstOrDefault()!.Mcc
            })
            .OrderBy(x => x.Amount);

        // var freqLogosCommand = new FetchMostFrequencyIconsCommand(grouped.Select(x => x.Name));
        // await _sender.Send(freqLogosCommand, cancellationToken);

        var result = _mapper.Map<List<ClientTransactionsGroupedResponse>>(grouped);
        return result;
    }
}