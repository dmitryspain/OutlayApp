using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.Live;
using OutlayApp.Infrastructure.BackgroundJobs;

namespace OutlayApp.Infrastructure.Live;

public static class DependencyInjection
{
    /// <summary>Live events for the UI (Postgres NOTIFY) and the durable history worker.</summary>
    public static IServiceCollection AddLiveUpdates(this IServiceCollection services)
    {
        services.AddSingleton<LocalLiveHub>();
        services.AddSingleton<PostgresLiveEvents>();
        services.AddSingleton<ILiveEvents>(sp => sp.GetRequiredService<PostgresLiveEvents>());
        services.AddHostedService(sp => sp.GetRequiredService<PostgresLiveEvents>());
        services.AddScoped<IBackfillQueue, BackfillQueue>();
        services.AddHostedService<BackfillWorker>();
        return services;
    }
}
