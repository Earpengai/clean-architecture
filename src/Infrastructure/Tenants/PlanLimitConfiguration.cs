using Domain.Subscriptions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Tenants;

internal sealed class PlanLimitConfiguration : IEntityTypeConfiguration<PlanLimit>
{
    public void Configure(EntityTypeBuilder<PlanLimit> builder)
    {
        builder.HasKey(pl => new { pl.SubscriptionPlanId, pl.Limit });

        builder.Property(pl => pl.Limit).HasMaxLength(100);

        builder.HasOne(pl => pl.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(pl => pl.SubscriptionPlanId)
            .IsRequired();
    }
}
