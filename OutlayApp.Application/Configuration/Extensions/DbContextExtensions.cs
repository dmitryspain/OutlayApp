using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.InMemoryDb;

namespace OutlayApp.Application.Configuration.Extensions;

public static class DbContextExtensions
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
        return services;
    }
}