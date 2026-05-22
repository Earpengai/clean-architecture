namespace Domain.Tenants;

public sealed class PlanLimit
{
    public SubscriptionPlan Plan { get; set; }
    public string Limit { get; set; }
    public int Value { get; set; }
}
