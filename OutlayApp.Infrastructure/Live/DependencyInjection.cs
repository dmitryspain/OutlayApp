using Microsoft.Extensions.DependencyInjection;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.Live;
using OutlayApp.Infrastructure.BackgroundJobs;

namespace OutlayApp.Infrastructure.Live;

public static class DependencyInjection
{
    /// <summary>Live events for the UI and the history backfill worker.</summary>
    public static IServiceCollection AddLiveUpdates(this IServiceCollection services)
    {
        services.AddSingleton<ILiveEvents, LiveEvents>();
        services.AddSingleton<BackfillQueue>();
        services.AddSingleton<IBackfillQueue>(sp => sp.GetRequiredService<BackfillQueue>());
        services.AddHostedService<BackfillWorker>();
        return services;
    }
}
