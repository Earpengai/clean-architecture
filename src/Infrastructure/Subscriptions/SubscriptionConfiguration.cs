using Domain.Subscriptions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Subscriptions;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Tenant)
            .WithOne(t => t.Subscription)
            .HasForeignKey<Subscription>(s => s.TenantId)
            .IsRequired();

        builder.HasOne(s => s.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(s => s.SubscriptionPlanId)
            .IsRequired();

        builder.Property(s => s.Status).HasConversion<string>();
        builder.Property(s => s.BillingPeriod).HasConversion<string>();
    }
}
