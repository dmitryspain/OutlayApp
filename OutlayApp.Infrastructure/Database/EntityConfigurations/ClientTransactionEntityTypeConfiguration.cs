using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class ClientTransactionEntityTypeConfiguration : IEntityTypeConfiguration<ClientTransaction>
{
    public void Configure(EntityTypeBuilder<ClientTransaction> builder)
    {
        builder.HasKey(x => x.Id);
    }
}