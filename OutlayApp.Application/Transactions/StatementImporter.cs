using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Repositories;

namespace OutlayApp.Application.Transactions;

/// <param name="Added">new rows (not yet saved)</param>
/// <param name="Updated">stored rows that took over a bank id or got settled</param>
/// <param name="RemovedDuplicates">stored copies of one bank item, dropped</param>
public sealed record ImportResult(List<ClientTransaction> Added, int Updated, int RemovedDuplicates);

/// <summary>
/// Stores bank statement items on a card. Webhooks, polling and backfills all go through here, so:
///  - an item already stored (same bank id) is never added again — but a hold that settled is updated;
///  - a row stored before bank ids were kept is matched by content and takes over the id; extra copies
///    of the same item left by the old importer are removed.
/// Nothing is saved here: the caller commits.
/// </summary>
public sealed class StatementImporter
{
    private readonly IClientTransactionRepository _transactions;

    public StatementImporter(IClientTransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<ImportResult> Import(ClientCard card, IReadOnlyCollection<MonobankTransaction> items,
        CancellationToken cancellationToken)
    {
        var added = new List<ClientTransaction>();
        if (items.Count == 0)
            return new ImportResult(added, 0, 0);

        var from = items.Min(x => x.TimeUtc).AddSeconds(-1);
        var to = items.Max(x => x.TimeUtc).AddSeconds(1);
        var stored = await _transactions.GetByPeriod(card.Id, from, to, cancellationToken);

        var byId = stored.Where(x => x.ExternalId is not null).ToDictionary(x => x.ExternalId!);
        var legacy = stored.Where(x => x.ExternalId is null)
            .GroupBy(Key)
            .ToDictionary(g => g.Key, g => new Queue<ClientTransaction>(g));
        var claimedKeys = new HashSet<(DateTime, decimal, string)>();
        var updated = 0;

        foreach (var item in items)
        {
            var details = item.ToDetails();
            if (byId.TryGetValue(item.Id, out var existing))
            {
                if (existing.Hold && !item.Hold)
                {
                    existing.Settle();
                    updated++;
                }
                continue;
            }

            var key = Key(details);
            if (legacy.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                var row = queue.Dequeue();
                row.AdoptBankItem(details);
                byId[item.Id] = row;
                claimedKeys.Add(key);
                updated++;
                continue;
            }

            var result = card.AddTransaction(details);
            if (result.IsFailure)
                continue;
            byId[item.Id] = result.Value!;
            added.Add(result.Value!);
        }

        // the bank listed this content N times and we matched N rows: whatever is left over is a copy
        var removed = 0;
        foreach (var key in claimedKeys)
        {
            foreach (var copy in legacy[key])
            {
                _transactions.Remove(copy);
                removed++;
            }
        }

        await _transactions.AddRange(added, cancellationToken);
        return new ImportResult(added, updated, removed);
    }

    private static (DateTime, decimal, string) Key(ClientTransaction t) => (t.DateOccured, t.Amount, t.Description);
    private static (DateTime, decimal, string) Key(TransactionDetails d) => (d.DateOccuredUtc, d.Amount, d.Description);
}
