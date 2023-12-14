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
        CancellationToken cancellationToken = default)
    {
        return _context.ClientTransactions.Where(x =>
                x.ClientCardId == clientCardId && x.DateOccured > dateFrom && x.DateOccured < dateTo)
            .OrderByDescending(x => x.DateOccured)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ClientTransaction>> GetByDescription(Guid clientCardId, string description,
        CancellationToken cancellationToken = default)
    {
        return _context.ClientTransactions.Where(x => x.Description == description && x.ClientCardId == clientCardId)
            .OrderByDescending(x => x.DateOccured)
            .ToListAsync(cancellationToken);
    }

    public Task<ClientTransaction> GetLatest(Guid clientCardId, CancellationToken cancellationToken = default)
    {
        return _context.ClientTransactions.Where(x => x.ClientCardId == clientCardId)
            .OrderByDescending(x => x.DateOccured)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)!;
    }

    public Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default)
    {
        return _context.AddRangeAsync(transactions, cancellationToken);
    }
}