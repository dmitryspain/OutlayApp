using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.ClientCards;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class ClientCardEntityTypeConfiguration : IEntityTypeConfiguration<ClientCard>
{
    public void Configure(EntityTypeBuilder<ClientCard> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Balance).HasPrecision(18, 2);
        builder.Property(x => x.CreditLimit).HasPrecision(18, 2);
        builder.HasIndex(x => x.ExternalCardId);
    }
}
