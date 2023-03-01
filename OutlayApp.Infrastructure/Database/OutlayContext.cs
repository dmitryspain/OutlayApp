using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.CompanyLogoReferences;
using OutlayApp.Infrastructure.Processing.Outbox;

namespace OutlayApp.Infrastructure.Database;

public class OutlayContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientTransaction> ClientTransactions { get; set; }
    public DbSet<ClientCard> ClientCards { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<LogoReference> LogoReferences { get; set; }
    public DbSet<InvalidReference> InvalidReferences { get; set; }

    public OutlayContext(DbContextOptions<OutlayContext> options) 
        : base(options)
    {
        // Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}