using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Repositories;

namespace OutlayApp.Tests;

/// <summary>In-memory transactions of the cards under test.</summary>
internal sealed class FakeTransactionRepository : IClientTransactionRepository
{
    public List<ClientTransaction> Rows { get; } = new();

    public Task<List<ClientTransaction>> GetByPeriod(Guid clientCardId, DateTime dateFrom, DateTime dateTo,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Rows.Where(x => x.ClientCardId == clientCardId && x.DateOccured > dateFrom && x.DateOccured < dateTo)
            .OrderByDescending(x => x.DateOccured).ToList());

    public Task<List<ClientTransaction>> GetByDescription(Guid clientCardId, string description, DateTime dateFrom,
        DateTime dateTo, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<ClientTransaction?> GetLatest(Guid clientCardId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Rows.Where(x => x.ClientCardId == clientCardId).MaxBy(x => x.DateOccured));

    public Task<ClientTransaction?> GetEarliest(Guid clientCardId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Rows.Where(x => x.ClientCardId == clientCardId).MinBy(x => x.DateOccured));

    public Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default)
    {
        Rows.AddRange(transactions);
        return Task.CompletedTask;
    }

    public void Remove(ClientTransaction transaction) => Rows.Remove(transaction);
}
