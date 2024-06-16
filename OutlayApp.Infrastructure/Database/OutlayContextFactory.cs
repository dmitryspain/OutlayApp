using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OutlayApp.Application.Configuration.Database;


namespace OutlayApp.Infrastructure.Database;

public class OutlayContextFactory : IDesignTimeDbContextFactory<OutlayContext>
{
    public OutlayContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();
        
        // var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
        // var configuration = new ConfigurationBuilder()
        //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //     .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
        //     .Build();
        
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<OutlayContext>();
       // dbContextOptionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=outlayappapi_db_1;Username=postgres;Password=postgres");
        dbContextOptionsBuilder.UseNpgsql(configuration[DbConnectionConstants.ConnectionString]);
        return new OutlayContext(dbContextOptionsBuilder.Options);
    }
}