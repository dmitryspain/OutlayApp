using Newtonsoft.Json;
using OutlayApp.Domain.SeedWork;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.Processing.Outbox;

namespace OutlayApp.Infrastructure.Processing;

public class DomainEventsDispatcher : IDomainEventsDispatcher
{
    private readonly OutlayContext _context;

    public DomainEventsDispatcher(OutlayContext context)
    {
        _context = context;
    }
    
    public Task DispatchEventsAsync()
    {
        var outboxMessages = _context.ChangeTracker
            .Entries<Entity>()
            .Select(x => x.Entity)
            .SelectMany(aggregateRoot => aggregateRoot.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(domainEvent,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    })
            })
            .ToList();

        return _context.Set<OutboxMessage>().AddRangeAsync(outboxMessages);
    }
}