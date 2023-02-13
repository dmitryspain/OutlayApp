using Microsoft.EntityFrameworkCore;

namespace OutlayApp.Infrastructure.Database.InMemoryDb;

public class OutlayInMemoryContext : DbContext
{
    public OutlayInMemoryContext(DbContextOptions<OutlayInMemoryContext> options)
        : base(options)
    {
        Database.EnsureDeleted();
    }

    public DbSet<MccInfo> MccInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MccInfo>().HasKey(x => x.Mcc);
    }
}