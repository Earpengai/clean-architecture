using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Tenants;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.Identifier).IsUnique();

        builder.Property(t => t.StripeCustomerId).HasMaxLength(255);
        builder.Property(t => t.StripeSubscriptionId).HasMaxLength(255);

        builder.Property(t => t.BillingPeriod).HasConversion<string>();
        builder.Property(t => t.SubscriptionExpiresAt);
    }
}
