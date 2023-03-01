using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Processing;

namespace OutlayApp.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly OutlayContext _context;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public UnitOfWork(OutlayContext context, IDomainEventsDispatcher domainEventsDispatcher)
    {
        _context = context;
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _domainEventsDispatcher.DispatchEventsAsync();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public IDbTransaction BeginTransaction()
    {
        var transaction = _context.Database.BeginTransaction();
        Current = _context.Database.CurrentTransaction;
        return transaction.GetDbTransaction();
    }

    public IDbContextTransaction? Current { get; set; }
}