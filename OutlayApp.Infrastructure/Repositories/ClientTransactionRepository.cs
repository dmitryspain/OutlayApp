using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.Repositories;

public class ClientTransactionRepository : IClientTransactionRepository
{
    private readonly OutlayContext _context;

    public ClientTransactionRepository(OutlayContext context)
    {
        _context = context;
    }

    public Task<List<ClientTransaction>> GetByPeriod(Guid clientCardId, DateTime dateFrom, DateTime dateTo,
        CancellationToken cancellationToken = default) =>
        _context.ClientTransactions
            .Where(x => x.ClientCardId == clientCardId && x.DateOccured > dateFrom && x.DateOccured < dateTo)
            .OrderByDescending(x => x.DateOccured)
            .ToListAsync(cancellationToken);

    public Task<List<ClientTransaction>> GetByDescription(Guid clientCardId, string description,
        DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default) =>
        _context.ClientTransactions
            .Where(x => x.ClientCardId == clientCardId && x.Description == description &&
                        x.DateOccured > dateFrom && x.DateOccured < dateTo)
            .OrderByDescending(x => x.DateOccured)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<ClientTransaction?> GetLatest(Guid clientCardId, CancellationToken cancellationToken = default) =>
        _context.ClientTransactions.Where(x => x.ClientCardId == clientCardId)
            .OrderByDescending(x => x.DateOccured)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ClientTransaction?> GetEarliest(Guid clientCardId, CancellationToken cancellationToken = default) =>
        _context.ClientTransactions.Where(x => x.ClientCardId == clientCardId)
            .OrderBy(x => x.DateOccured)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

    public Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default) =>
        _context.AddRangeAsync(transactions, cancellationToken);

    public void Remove(ClientTransaction transaction) => _context.ClientTransactions.Remove(transaction);
}
