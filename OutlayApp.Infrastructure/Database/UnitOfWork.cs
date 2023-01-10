using OutlayApp.Domain.Repositories;

namespace OutlayApp.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly OutlayContext _context;
    // private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public UnitOfWork(OutlayContext context)
    {
        _context = context;
        // _domainEventsDispatcher = domainEventsDispatcher;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}