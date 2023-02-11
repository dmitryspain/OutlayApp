using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Infrastructure.Database.InMemoryDb;

namespace OutlayApp.Infrastructure.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddInMemoryDbContext(this IServiceCollection services)
    {
        services.AddDbContext<OutlayInMemoryContext>(
            options => options.UseInMemoryDatabase(databaseName: "OutlayInMemoryContext"), ServiceLifetime.Singleton);
        var context = services.BuildServiceProvider().GetRequiredService<OutlayInMemoryContext>();
        MccInfoInitializer.AddMccs(context, CancellationToken.None).Wait();
        return services;
    }
    
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OutlayContext>(
            options => options.UseNpgsql(configuration[DbConnectionConstants.ConnectionString]));
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        return services;
    }
}