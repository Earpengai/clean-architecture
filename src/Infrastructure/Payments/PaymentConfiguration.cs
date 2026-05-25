using Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Payments;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.Md5Hash);

        builder.HasIndex(p => p.TenantId);

        builder.Property(p => p.Md5Hash).HasMaxLength(100);
        builder.Property(p => p.Currency).HasMaxLength(10);
        builder.Property(p => p.TransactionHash).HasMaxLength(128);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.BillingPeriod).HasConversion<string>();
        builder.Property(p => p.Status).HasConversion<string>();

        builder.HasOne(p => p.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(p => p.SubscriptionPlanId)
            .IsRequired();
    }
}
