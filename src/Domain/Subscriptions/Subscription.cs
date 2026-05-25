using Domain.Tenants;

namespace Domain.Subscriptions;

public sealed class Subscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public SubscriptionBillingPeriod BillingPeriod { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUsersOverride { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Tenant? Tenant { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
