using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Domain.Repositories;

public interface IClientTransactionRepository : IRepository<ClientTransaction>
{
    /// <summary>Exclusive bounds, UTC, newest first.</summary>
    Task<List<ClientTransaction>> GetByPeriod(Guid clientCardId, DateTime dateFrom, DateTime dateTo,
        CancellationToken cancellationToken = default);

    Task<List<ClientTransaction>> GetByDescription(Guid clientCardId, string description,
        DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default);

    Task<ClientTransaction?> GetLatest(Guid clientCardId, CancellationToken cancellationToken = default);
    Task<ClientTransaction?> GetEarliest(Guid clientCardId, CancellationToken cancellationToken = default);
    Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default);
    void Remove(ClientTransaction transaction);
}
