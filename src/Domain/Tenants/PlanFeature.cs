namespace Domain.Tenants;

public sealed class PlanFeature
{
    public SubscriptionPlan Plan { get; set; }
    public string Feature { get; set; }
    public bool IsEnabled { get; set; }
}
