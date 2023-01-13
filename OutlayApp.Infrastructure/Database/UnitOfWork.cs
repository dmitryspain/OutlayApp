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
}