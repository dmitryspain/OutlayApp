using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Application.Security;
using OutlayApp.Infrastructure.Security;

namespace OutlayApp.Infrastructure.Database;

public static class DependencyInjection
{
    /// <summary>Postgres (all dates UTC, <c>timestamptz</c>), token encryption keys stored in it.</summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration[DbConnectionConstants.ConnectionString]
                               ?? throw new InvalidOperationException($"{DbConnectionConstants.ConnectionString} is not set");

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        services.AddDbContext<OutlayContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

        services.AddDataProtection()
            .SetApplicationName("Outlay")
            .PersistKeysToDbContext<OutlayContext>();
        services.AddSingleton<ITokenProtector, TokenProtector>();
        services.AddHostedService<LegacyTokenMigrator>();
        return services;
    }
}
