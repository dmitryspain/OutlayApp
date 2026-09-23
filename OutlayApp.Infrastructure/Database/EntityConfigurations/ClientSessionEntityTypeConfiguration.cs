using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.Sessions;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class ClientSessionEntityTypeConfiguration : IEntityTypeConfiguration<ClientSession>
{
    public void Configure(EntityTypeBuilder<ClientSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasOne<Client>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
    }
}
