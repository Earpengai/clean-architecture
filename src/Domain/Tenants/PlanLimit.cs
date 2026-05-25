using Domain.Subscriptions;

namespace Domain.Tenants;

public sealed class PlanLimit
{
    public Guid SubscriptionPlanId { get; set; }
    public string Limit { get; set; }
    public int Value { get; set; }

    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
