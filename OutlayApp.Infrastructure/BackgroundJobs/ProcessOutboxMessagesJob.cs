using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OutlayApp.Application.Abstractions;
using OutlayApp.Domain.Primitives;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.Processing.Outbox;
using Quartz;

namespace OutlayApp.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private const int MaxAttempts = 10;
    private readonly OutlayContext _context;
    private readonly IPublisher _publisher;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(OutlayContext context, IPublisher publisher, ILogger<ProcessOutboxMessagesJob> logger)
    {
        _context = context;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await _context
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                message.Content,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
            );

            try
            {
                if (domainEvent is not null)
                    await _publisher.Publish(domainEvent, context.CancellationToken);
                message.ProcessedOnUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                message.Error = null;
            }
            catch (RetryLaterException ex)
            {
                // left unprocessed: the next run (every 10 s) tries again
                message.Error = ex.Message;
                _logger.LogInformation("Outbox message {Id} postponed: {Reason}", message.Id, ex.Message);
            }
            catch (Exception ex)
            {
                // one bad message must not block the others; give up on it after a few tries
                message.Error = ex.Message;
                message.Attempts++;
                if (message.Attempts >= MaxAttempts)
                    message.ProcessedOnUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _logger.LogError(ex, "Outbox message {Id} failed", message.Id);
            }

            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
