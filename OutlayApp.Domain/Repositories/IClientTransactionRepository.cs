using OutlayApp.Domain.Clients.Transactions;

namespace OutlayApp.Domain.Repositories;

public interface IClientTransactionRepository : IRepository<ClientTransaction>
{
    Task<List<ClientTransaction>> GetByPeriod(DateTime dateFrom, DateTime dateTo,
        CancellationToken cancellationToken = default);
    Task<ClientTransaction?> GetLatest(CancellationToken cancellationToken = default);
    Task Add(ClientTransaction transaction);
    Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default);
}