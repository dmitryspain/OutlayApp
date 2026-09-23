using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Infrastructure.BackgroundJobs;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class BackfillJobEntityTypeConfiguration : IEntityTypeConfiguration<BackfillJob>
{
    public void Configure(EntityTypeBuilder<BackfillJob> builder)
    {
        builder.HasKey(x => x.CardId);
        builder.HasIndex(x => new { x.State, x.NextRunAtUtc });
        builder.Property(x => x.State).HasMaxLength(16);
    }
}
