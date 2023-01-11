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

    public Task<List<ClientTransaction>> GetByPeriod(DateTime dateFrom, DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return _context.ClientTransactions.Where(x => x.DateOccured > dateFrom && x.DateOccured < dateTo)
            .ToListAsync(cancellationToken);
    }

    public Task<ClientTransaction?> GetLatest(Guid clientId, CancellationToken cancellationToken = default)
    {
        return _context.ClientTransactions.Where(x => x.ClientId == clientId)
            .OrderBy(x => x.DateOccured)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task Add(ClientTransaction transaction)
    {
        return _context.AddAsync(transaction).AsTask();
    }

    public Task AddRange(IEnumerable<ClientTransaction> transactions, CancellationToken cancellationToken = default)
    {
        return _context.AddRangeAsync(transactions, cancellationToken);
    }
}