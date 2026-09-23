using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.Clients;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class ClientEntityTypeConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(x => x.Id);
        // the old plain-text column; emptied at startup, see LegacyTokenMigrator
        builder.Property(x => x.LegacyPlainToken).HasColumnName("PersonalToken");
        builder.HasIndex(x => x.TokenHash).IsUnique().HasFilter("\"TokenHash\" IS NOT NULL");
        builder.HasMany(x => x.Cards).WithOne().HasForeignKey(x => x.ClientId);
        builder.Navigation(x => x.Cards).HasField("_cards");
    }
}
