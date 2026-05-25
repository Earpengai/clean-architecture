using Domain.Subscriptions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Tenants;

internal sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.HasKey(pf => new { pf.SubscriptionPlanId, pf.Feature });

        builder.Property(pf => pf.Feature).HasMaxLength(100);

        builder.HasOne(pf => pf.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(pf => pf.SubscriptionPlanId)
            .IsRequired();
    }
}
