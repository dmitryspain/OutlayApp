using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.Clients.Cards;
using OutlayApp.Domain.Clients.Transactions;

namespace OutlayApp.Infrastructure.Database;

public class OutlayContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientTransaction> ClientTransactions { get; set; }
    public DbSet<ClientCard> ClientCards { get; set; }

    public OutlayContext(DbContextOptions options) 
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}