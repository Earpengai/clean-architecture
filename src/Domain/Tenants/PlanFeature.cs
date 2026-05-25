using Domain.Subscriptions;

namespace Domain.Tenants;

public sealed class PlanFeature
{
    public Guid SubscriptionPlanId { get; set; }
    public string Feature { get; set; }
    public bool IsEnabled { get; set; }

    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
