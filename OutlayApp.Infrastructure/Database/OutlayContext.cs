using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.CompanyLogoReferences;
using OutlayApp.Domain.Sessions;
using OutlayApp.Infrastructure.BackgroundJobs;
using OutlayApp.Infrastructure.Processing.Outbox;

namespace OutlayApp.Infrastructure.Database;

public class OutlayContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<ClientTransaction> ClientTransactions { get; set; } = null!;
    public DbSet<ClientCard> ClientCards { get; set; } = null!;
    public DbSet<ClientSession> ClientSessions { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<LogoReference> LogoReferences { get; set; } = null!;
    public DbSet<InvalidReference> InvalidReferences { get; set; } = null!;
    public DbSet<BackfillJob> BackfillJobs { get; set; } = null!;
    /// <summary>Keys that encrypt Monobank tokens — shared by every instance.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    public OutlayContext(DbContextOptions<OutlayContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
