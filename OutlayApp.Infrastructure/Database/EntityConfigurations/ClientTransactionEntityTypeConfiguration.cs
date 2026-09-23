using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class ClientTransactionEntityTypeConfiguration : IEntityTypeConfiguration<ClientTransaction>
{
    public void Configure(EntityTypeBuilder<ClientTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        // every query filters by card; the filtered index below cannot serve that
        builder.HasIndex(x => x.ClientCardId);
        // webhook, polling and history backfill can all deliver the same item
        builder.HasIndex(x => new { x.ClientCardId, x.ExternalId })
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL");
    }
}