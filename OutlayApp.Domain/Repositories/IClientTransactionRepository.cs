using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Domain.Repositories;

public interface IClientTransactionRepository : IRepository<ClientTransaction>
{
    Task<List<ClientTransaction>> GetByPeriod(Guid clientCardId, DateTime dateFrom, DateTime dateTo,
        CancellationToken cancellationToken = default);
    
    Task<List<ClientTransaction>> GetByDescription(Guid clientCardId, string description,
        CancellationToken cancellationToken = default);
    
    Task<ClientTransaction> GetLatest(Guid clientCardId, CancellationToken cancellationToken = default);
    Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default);
}