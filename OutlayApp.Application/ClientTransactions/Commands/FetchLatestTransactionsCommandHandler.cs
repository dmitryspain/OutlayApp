using System.Net.Http.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Configuration.Extensions;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.LogoReferences;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientTransactions.Commands;

public class FetchLatestTransactionsCommandHandler : ICommandHandler<FetchLatestTransactionsCommand>
{
    #region Properties


    private const int MaxDaysPeriod = 30;
    private const int FirstLogosFetchCount = 10;
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly ILogger<FetchLatestTransactionsCommandHandler> _logger;
    private readonly IClientCardsRepository _cardsRepository;
    private readonly IClientTransactionRepository _transactionRepository;
    private readonly IClientRepository _clientRepository;

    #endregion

    public FetchLatestTransactionsCommandHandler(IHttpClientFactory factory, IClientCardsRepository cardsRepository,
        IClientTransactionRepository transactionRepository, IClientRepository clientRepository, IUnitOfWork unitOfWork,
        ISender sender, ILogger<FetchLatestTransactionsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _sender = sender;
        _logger = logger;
        _cardsRepository = cardsRepository;
        _transactionRepository = transactionRepository;
        _clientRepository = clientRepository;
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
    }

    public async Task<Result> Handle(FetchLatestTransactionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clientCard = await _cardsRepository.GetById(request.CardId, cancellationToken);
            if (clientCard is null)
                return Result.Failure(new Error("ClientCard.NotFound",
                    $"No client card with External Id {request.CardId}"));

            long unixTimeFrom;
            var latest = await _transactionRepository.GetLatest(clientCard.Id, cancellationToken);
            if (latest is null)
                unixTimeFrom = DateTimeOffset.Now.AddDays(-MaxDaysPeriod).ToUnixTimeSeconds();
            else
                unixTimeFrom = DateTimeOffset.Now - latest.DateOccured
                               < TimeSpan.FromDays(MaxDaysPeriod)
                    ? ((DateTimeOffset)latest!.DateOccured).ToUnixTimeSeconds() + 1
                    : DateTimeOffset.Now.AddDays(-MaxDaysPeriod).ToUnixTimeSeconds();

            var unixTimeTo = DateTimeOffset.Now.ToUnixTimeSeconds();
            var url = BuildUrl(clientCard.ExternalCardId, unixTimeFrom, unixTimeTo);
            var client = await _clientRepository.GetById(clientCard.ClientId, cancellationToken);
            _httpClient.DefaultRequestHeaders.Add(MonobankConstants.TokenHeader, client.PersonalToken);

            var result = await _httpClient.GetAsync(url, cancellationToken);
            var mes = result.Content.ReadAsStringAsync(cancellationToken);
            var monobankTransactions = (await result.Content.ReadFromJsonAsync<IEnumerable<MonobankTransaction>>(
                cancellationToken: cancellationToken) ?? Array.Empty<MonobankTransaction>()).ToList();

            foreach (var transaction in monobankTransactions)
            {
                var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev");
                var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(transaction.Time).DateTime, TimeZoneInfo.Utc, localTimeZone);

                var transactionResult = clientCard.AddTransaction(
                    transaction.Description,
                    transaction.Amount.ToDecimal(),
                    transaction.Balance.ToDecimal(),
                    localTime,
                    transaction.Mcc);

                if (transactionResult.IsFailure)
                {
                    return Result.Failure(
                        new Error("ClientTransaction.CreationFailed", "Client transaction creation is failed"));
                }
            }

            var mostFrequencyTransactions = monobankTransactions
                .Where(x => x.Mcc != MccsConstants.MoneyTransfer) 
                .GroupBy(x => x.Description)
                .OrderByDescending(x => x.Count())
                .Take(FirstLogosFetchCount)
                .Select(x => x.Key);

            var freqLogosCommand = new FetchMostFrequencyIconsCommand(mostFrequencyTransactions);
            
            await _sender.Send(freqLogosCommand, cancellationToken);
            await _transactionRepository.AddRange(clientCard.Transactions, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error");
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
        }
        
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