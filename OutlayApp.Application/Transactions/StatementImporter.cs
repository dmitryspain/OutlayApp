using System.Net;
using System.Net.Http.Json;
using OutlayApp.Application.Configuration.Extensions;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Repositories;

namespace OutlayApp.Application.Transactions;

/// <summary>
/// Reads statement pages from Monobank and stores the items a card does not have yet.
/// Webhooks, polling and the history backfill all go through here, so an item is never stored twice.
/// </summary>
public sealed class StatementImporter
{
    private readonly HttpClient _httpClient;
    private readonly IClientTransactionRepository _transactionRepository;

    public StatementImporter(IHttpClientFactory factory, IClientTransactionRepository transactionRepository)
    {
        _httpClient = factory.CreateClient(MonobankConstants.HttpClient);
        _transactionRepository = transactionRepository;
    }

    /// <summary>One statement page, newest first. Throws <see cref="MonobankRateLimitException"/> on 429.</summary>
    public async Task<List<MonobankTransaction>> FetchPage(string token, string externalCardId, long from, long to,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/personal/statement/{externalCardId}/{from}/{to}");
        request.Headers.Add(MonobankConstants.TokenHeader, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new MonobankRateLimitException();
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<MonobankTransaction>>(cancellationToken: cancellationToken)
               ?? new List<MonobankTransaction>();
    }

    /// <summary>Adds the items the card does not have yet (not saved — the caller commits). Returns what was added.</summary>
    public async Task<List<ClientTransaction>> AddNew(ClientCard card, IReadOnlyCollection<MonobankTransaction> items,
        CancellationToken cancellationToken)
    {
        var added = new List<ClientTransaction>();
        if (items.Count == 0)
            return added;

        var from = items.Min(x => x.LocalTime).AddSeconds(-1);
        var to = items.Max(x => x.LocalTime).AddSeconds(1);
        var stored = await _transactionRepository.GetByPeriod(card.Id, from, to, cancellationToken);

        var knownIds = stored.Where(x => x.ExternalId is not null).Select(x => x.ExternalId!).ToHashSet();
        // rows stored before ExternalId existed are matched by their content
        var knownLegacy = stored.Where(x => x.ExternalId is null)
            .Select(x => (x.DateOccured, x.Amount, x.Description))
            .ToHashSet();

        foreach (var item in items)
        {
            var amount = item.Amount.ToDecimal();
            if (knownIds.Contains(item.Id) || knownLegacy.Contains((item.LocalTime, amount, item.Description)))
                continue;

            var result = card.AddTransaction(item.Description, amount, item.Balance.ToDecimal(), item.LocalTime,
                item.Mcc, item.Id);
            if (result.IsFailure)
                continue;

            knownIds.Add(item.Id);
            added.Add(result.Value);
        }

        await _transactionRepository.AddRange(added, cancellationToken);
        return added;
    }
}
