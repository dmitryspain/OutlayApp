using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.CompanyLogoReferences;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class InvalidReferenceEntityTypeConfiguration : IEntityTypeConfiguration<InvalidReference>
{
    public void Configure(EntityTypeBuilder<InvalidReference> builder)
    {
        builder.HasKey(x => x.Id);
    }
}