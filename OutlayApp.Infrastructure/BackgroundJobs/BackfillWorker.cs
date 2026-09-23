using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Live;
using OutlayApp.Application.Monobank;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.BackgroundJobs;

/// <summary>
/// Works through <see cref="BackfillJob"/>s one statement window at a time. A job is claimed with a row lock
/// (FOR UPDATE SKIP LOCKED), so several instances can run this; the next window of a job waits the bank's
/// one-request-per-minute interval; progress is saved after every window, so a restart just continues.
/// </summary>
public sealed class BackfillWorker : BackgroundService
{
    private static readonly TimeSpan Poll = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(2);
    private const int MaxAttempts = 5;

    private readonly IServiceScopeFactory _scopes;
    private readonly ILiveEvents _live;
    private readonly ILogger<BackfillWorker> _logger;

    public BackfillWorker(IServiceScopeFactory scopes, ILiveEvents live, ILogger<BackfillWorker> logger)
    {
        _scopes = scopes;
        _live = live;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await RunOne(stoppingToken))
                    await Task.Delay(Poll, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backfill worker iteration failed");
                await Task.Delay(Poll, stoppingToken);
            }
        }
    }

    /// <summary>Claims one due job and imports one window of it. False when nothing was due.</summary>
    private async Task<bool> RunOne(CancellationToken ct)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OutlayContext>();

        var now = DateTime.UtcNow;
        var claimed = await db.BackfillJobs
            .FromSqlInterpolated($"""
                UPDATE "BackfillJobs" SET "LockedUntilUtc" = {now + Lease}
                WHERE "CardId" = (
                    SELECT "CardId" FROM "BackfillJobs"
                    WHERE "State" = {BackfillStates.Running} AND "NextRunAtUtc" <= {now}
                      AND ("LockedUntilUtc" IS NULL OR "LockedUntilUtc" < {now})
                    ORDER BY "NextRunAtUtc"
                    LIMIT 1
                    FOR UPDATE SKIP LOCKED)
                RETURNING *
                """)
            .AsNoTracking()
            .ToListAsync(ct);
        if (claimed.Count == 0)
            return false;

        var job = await db.BackfillJobs.FirstAsync(x => x.CardId == claimed[0].CardId, ct);
        try
        {
            var result = await scope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new ImportStatementWindowCommand(job.CardId, job.CursorTo, job.Floor), ct);
            if (result.IsFailure)
            {
                Fail(job, result.Error.Message);
            }
            else
            {
                job.Imported += result.Value!.Added;
                job.CursorTo = result.Value.NextTo;
                job.Attempts = 0;
                job.Error = null;
                if (result.Value.Finished)
                    job.State = BackfillStates.Done;
                job.NextRunAtUtc = DateTime.UtcNow + MonobankConstants.RequestInterval;
            }
        }
        catch (MonobankRateLimitException ex)
        {
            // somebody else used the token's minute (a refresh, another job): just come back later
            job.NextRunAtUtc = DateTime.UtcNow + ex.RetryAfter;
        }
        catch (MonobankException ex)
        {
            if (++job.Attempts >= MaxAttempts)
                Fail(job, ex.Message);
            else
                job.NextRunAtUtc = DateTime.UtcNow + MonobankConstants.RequestInterval * job.Attempts;
        }

        job.LockedUntilUtc = null;
        job.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await _live.Publish(job.CardId, new LiveEvent(LiveEventTypes.Backfill, BackfillQueue.ToStatus(job)));
        if (job.State == BackfillStates.Done)
            _logger.LogInformation("History job for card {CardId} done: {Count} transactions", job.CardId, job.Imported);
        return true;
    }

    private void Fail(BackfillJob job, string error)
    {
        job.State = BackfillStates.Failed;
        job.Error = error;
        _logger.LogWarning("History job for card {CardId} failed: {Error}", job.CardId, error);
    }
}
