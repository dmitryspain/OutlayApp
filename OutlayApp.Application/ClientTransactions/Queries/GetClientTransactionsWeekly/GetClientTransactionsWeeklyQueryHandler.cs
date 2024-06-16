using AutoMapper;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsWeekly;

public class GetClientTransactionsWeeklyQueryHandler : IQueryHandler<GetClientTransactionsWeeklyQuery,
    List<ClientTransactionsWeeklyResponse>>
{
    private readonly IClientTransactionRepository _clientTransactionRepository;
    private readonly IMapper _mapper;

    public GetClientTransactionsWeeklyQueryHandler(IClientTransactionRepository clientTransactionRepository,
        IMapper mapper)
    {
        _clientTransactionRepository = clientTransactionRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ClientTransactionsWeeklyResponse>>> Handle(GetClientTransactionsWeeklyQuery request,
        CancellationToken cancellationToken)
    {
        const int daysInWeek = 7;
        var currDay = (int)DateTime.Now.DayOfWeek;
        var dayStart = daysInWeek * request.SkipWeeks;

        // Calculate the starting and ending dates for the 7-day period
        var dateStart = DateTime.Now.Date.AddDays(-currDay).AddDays(-dayStart);
        var dateEnd = dateStart.AddDays(daysInWeek);

        var transactions = await _clientTransactionRepository.GetByPeriod(request.ClientCardId,
            dateStart, dateEnd, cancellationToken);

        var transactionDtos = _mapper.Map<List<ClientTransactionDto>>(transactions);

        var grouped = transactionDtos.GroupBy(x => x.DateOccured.DayOfWeek).Select(x =>
                new ClientTransactionsWeeklyResponse
                {
                    DayOfWeek = x.Key,
                    Amount = x.Sum(dto => dto.Amount),
                    Transactions = x.Select(t => t).ToList()
                }).OrderBy(x => x.DayOfWeek)
            .ToList();

        return grouped;
    }
}