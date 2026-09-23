using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Transactions;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Infrastructure.BackgroundJobs;

/// <summary>
/// Walks a card's history backwards one statement window at a time. Monobank allows one statement
/// request per minute per token, so windows are spaced out and a 429 simply waits and retries.
/// </summary>
public sealed class BackfillWorker : BackgroundService
{
    private static readonly TimeSpan RequestGap = TimeSpan.FromSeconds(61);
    private const int MaxRateLimitRetries = 5;

    private readonly BackfillQueue _queue;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<BackfillWorker> _logger;

    public BackfillWorker(BackfillQueue queue, IServiceScopeFactory scopes, ILogger<BackfillWorker> logger)
    {
        _queue = queue;
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (cardId, months) in _queue.Jobs.ReadAllAsync(stoppingToken))
        {
            try
            {
                await Run(cardId, months, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "History backfill failed for card {CardId}", cardId);
                var last = _queue.Get(cardId);
                _queue.Report(last with { State = BackfillStates.Failed, EtaSeconds = null, Error = ex.Message });
            }
        }
    }

    private async Task Run(Guid cardId, int months, CancellationToken ct)
    {
        var to = await Send(new GetHistoryStartQuery(cardId), ct);
        var start = to;
        var floor = DateTimeOffset.Now.AddMonths(-months).ToUnixTimeSeconds();
        var imported = 0;
        var retries = 0;

        while (to > floor)
        {
            _queue.Report(Status(cardId, BackfillStates.Running, start, to, floor, imported));

            ImportWindowResult window;
            try
            {
                window = await Send(new ImportStatementWindowCommand(cardId, to, floor), ct);
            }
            catch (MonobankRateLimitException) when (++retries <= MaxRateLimitRetries)
            {
                await Task.Delay(RequestGap, ct);
                continue;
            }

            retries = 0;
            imported += window.Added;
            to = window.NextTo;
            if (window.Finished)
                break;
            await Task.Delay(RequestGap, ct);
        }

        _queue.Report(Status(cardId, BackfillStates.Done, start, Math.Min(to, floor), floor, imported));
        _logger.LogInformation("History backfill for card {CardId} done: {Count} transactions", cardId, imported);
    }

    /// <summary>Each step gets its own scope, so the DbContext does not grow over a long run.</summary>
    private async Task<T> Send<T>(IRequest<Result<T>> request, CancellationToken ct)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ISender>().Send(request, ct);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Message);
        return result.Value!;
    }

    private static BackfillStatus Status(Guid cardId, string state, long start, long to, long floor, int imported)
    {
        var total = Math.Max(1, start - floor);
        var progress = state == BackfillStates.Done ? 1 : Math.Clamp((double)(start - to) / total, 0, 1);
        var windowsLeft = (int)Math.Ceiling((double)Math.Max(0, to - floor) / MonobankConstants.MaxStatementSeconds);
        return new BackfillStatus(cardId, state, progress, imported,
            DateTimeOffset.FromUnixTimeSeconds(to + 1).LocalDateTime,
            state == BackfillStates.Done ? null : windowsLeft * (int)RequestGap.TotalSeconds);
    }
}
