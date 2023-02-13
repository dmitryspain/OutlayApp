using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.CompanyLogoReferences;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class LogoReferenceEntityTypeConfiguration : IEntityTypeConfiguration<LogoReference>
{
    public void Configure(EntityTypeBuilder<LogoReference> builder)
    {
        builder.HasKey(x => x.Id);
    }
}