using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OutlayApp.Domain.Primitives;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.Processing.Outbox;
using Quartz;

namespace OutlayApp.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly OutlayContext _context;
    private readonly IPublisher _publisher;

    public ProcessOutboxMessagesJob(OutlayContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await _context
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                message.Content,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
            );

            if (domainEvent is null)
                continue;

            await _publisher.Publish(domainEvent, context.CancellationToken);
            message.ProcessedOnUtc = DateTimeOffset.Now.ToUnixTimeSeconds();
            await _context.SaveChangesAsync();
        }
    }
}

