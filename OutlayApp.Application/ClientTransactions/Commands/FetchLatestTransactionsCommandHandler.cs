using System.Net.Http.Json;
using AutoMapper;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Commands;

public class FetchLatestTransactionsCommandHandler : ICommandHandler<FetchLatestTransactionsCommand>
{
    private readonly IClientTransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private const int MaxDaysPeriod = 30;

    public
        FetchLatestTransactionsCommandHandler(IHttpClientFactory factory,
        IClientTransactionRepository transactionRepository, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
    }

    public async Task<Result> Handle(FetchLatestTransactionsCommand request, CancellationToken cancellationToken)
    {
        var latest = await _transactionRepository.GetLatest(request.ClientId, cancellationToken);
        long unixTimeFrom;
        if (latest is not null)
        {
            unixTimeFrom = DateTime.Now - latest.DateOccured < TimeSpan.FromDays(MaxDaysPeriod)
                ? ((DateTimeOffset)latest.DateOccured).ToUnixTimeSeconds()
                : DateTimeOffset.Now.AddDays(-MaxDaysPeriod).ToUnixTimeSeconds();
        }
        else
        {
            unixTimeFrom = DateTimeOffset.Now.AddDays(-MaxDaysPeriod).ToUnixTimeSeconds();
        }

        var unixTimeTo = DateTimeOffset.Now.ToUnixTimeSeconds();
        var url = $"/personal/statement/{request.ClientCardId}/{unixTimeFrom}/{unixTimeTo}";
        var result = await _httpClient.GetAsync(url, cancellationToken);
        var cardHistory = (await result.Content.ReadFromJsonAsync<IEnumerable<MonobankTransaction>>(cancellationToken:
            cancellationToken) ?? Array.Empty<MonobankTransaction>()).ToList();
        
        var clientTransactions = _mapper.Map<IEnumerable<ClientTransaction>>(cardHistory);
        await _transactionRepository.AddRange(clientTransactions, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    // private IEnumerable<ClientTransaction> MapToClientTransactions(IEnumerable<MonobankTransaction> transactions)
    // {
    //     foreach (var transaction in transactions)
    //     {
    //         yield return ClientTransaction.Create();
    //     }
    // }
}