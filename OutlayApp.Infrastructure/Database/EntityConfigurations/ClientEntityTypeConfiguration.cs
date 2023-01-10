using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutlayApp.Domain.Clients;

namespace OutlayApp.Infrastructure.Database.EntityConfigurations;

internal sealed class ClientEntityTypeConfiguration : IEntityTypeConfiguration<Client>
{
    // public void Configure(EntityTypeBuilder<Payment> builder)
    // {
    //     builder.ToTable("Payments", SchemaNames.Payments);
    //         
    //     builder.HasKey(b => b.Id);
    //
    //     builder.Property<DateTime>("_createDate").HasColumnName("CreateDate");
    //     builder.Property<OrderId>("_orderId").HasColumnName("OrderId");
    //     builder.Property("_status").HasColumnName("StatusId").HasConversion(new EnumToNumberConverter<PaymentStatus, byte>());
    //     builder.Property<bool>("_emailNotificationIsSent").HasColumnName("EmailNotificationIsSent");
    // }
    
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(x => x.Id);
    }
}