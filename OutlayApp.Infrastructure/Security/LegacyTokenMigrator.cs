using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutlayApp.Application.Security;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.Security;

/// <summary>
/// One-time: clients stored before tokens were encrypted still have a plain token. Encrypt it, keep its hash
/// and clear the plain column. Runs before the app takes requests; a no-op once done.
/// </summary>
public sealed class LegacyTokenMigrator : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<LegacyTokenMigrator> _logger;

    public LegacyTokenMigrator(IServiceScopeFactory scopes, ILogger<LegacyTokenMigrator> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OutlayContext>();
        var protector = scope.ServiceProvider.GetRequiredService<ITokenProtector>();

        var legacy = await db.Clients.Where(c => c.LegacyPlainToken != null).ToListAsync(cancellationToken);
        if (legacy.Count == 0)
            return;
        foreach (var client in legacy)
            client.SetToken(client.LegacyPlainToken!, protector.Protect(client.LegacyPlainToken!));
        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Encrypted the Monobank tokens of {Count} clients", legacy.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
