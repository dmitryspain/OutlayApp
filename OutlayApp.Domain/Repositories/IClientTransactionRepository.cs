using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Domain.Repositories;

public interface IClientTransactionRepository : IRepository<ClientTransaction>
{
    Task<List<ClientTransaction>> GetByPeriod(Guid clientCardId, long dateFrom, long dateTo,
        CancellationToken cancellationToken = default);
    Task<ClientTransaction> GetLatest(Guid clientCardId, CancellationToken cancellationToken = default);
    Task Add(ClientTransaction transaction);
    Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default);
}