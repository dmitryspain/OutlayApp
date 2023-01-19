using System.Net.Http.Json;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Extensions;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Commands;

public class FetchLatestTransactionsCommandHandler : ICommandHandler<FetchLatestTransactionsCommand>
{
    private const int MaxDaysPeriod = 30;
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClientCardsRepository _cardsRepository;
    private readonly IClientTransactionRepository _transactionRepository;

    public FetchLatestTransactionsCommandHandler(IHttpClientFactory factory, IClientCardsRepository cardsRepository,
        IClientTransactionRepository transactionRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _cardsRepository = cardsRepository;
        _transactionRepository = transactionRepository;
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
    }

    public async Task<Result> Handle(FetchLatestTransactionsCommand request, CancellationToken cancellationToken)
    {
        var clientCard = await _cardsRepository.GetByExternalId(request.ExternalCardId, cancellationToken);
        if (clientCard is null)
            return Result.Failure(new Error("ClientCard.NotFound",
                $"No client card with External Id {request.ExternalCardId}"));

        long unixTimeFrom;
        var latest = await _transactionRepository.GetLatest(clientCard.Id, cancellationToken);
        if (latest is null)
            unixTimeFrom = DateTimeOffset.Now.AddDays(-MaxDaysPeriod).ToUnixTimeSeconds();
        else
            unixTimeFrom = DateTimeOffset.Now - DateTimeOffset.FromUnixTimeSeconds(latest.DateOccured)
                           < TimeSpan.FromDays(MaxDaysPeriod)
                ? latest.DateOccured + 1 // because we dont want to take the existing record from monobank api
                : DateTimeOffset.Now.AddDays(-MaxDaysPeriod).ToUnixTimeSeconds();

        var unixTimeTo = DateTimeOffset.Now.ToUnixTimeSeconds();
        var url = BuildUrl(clientCard.ExternalCardId, unixTimeFrom, unixTimeTo);
        var result = await _httpClient.GetAsync(url, cancellationToken);
        var monobankTransactions = (await result.Content.ReadFromJsonAsync<IEnumerable<MonobankTransaction>>(
            cancellationToken:
            cancellationToken) ?? Array.Empty<MonobankTransaction>()).ToList();

        if (monobankTransactions.Select(transaction => clientCard.AddTransaction(transaction.Description,
                transaction.Amount.ToDecimal(), transaction.Balance.ToDecimal(), transaction.Time, transaction.Mcc))
            .Any(transactionResult => transactionResult.IsFailure))
        {
            return Result.Failure(
                new Error("ClientTransaction.CreationFailed", "Client transaction creation is failed"));
        }

        await _transactionRepository.AddRange(clientCard.Transactions, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static string BuildUrl(string externalCardId, long? dateFrom = null, long? dateTo = null)
    {
        if (dateFrom is not null && dateTo is not null)
            return $"/personal/statement/{externalCardId}/{dateFrom}/{dateTo}";

        return dateFrom is not null
            ? $"/personal/statement/{externalCardId}/{dateFrom}/{DateTimeOffset.Now.ToUnixTimeSeconds()}"
            : $"/personal/statement/{externalCardId}/{DateTimeOffset.Now.ToUnixTimeSeconds()}";
    }
}