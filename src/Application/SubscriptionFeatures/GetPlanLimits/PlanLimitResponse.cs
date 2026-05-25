namespace Application.SubscriptionFeatures.GetPlanLimits;

public sealed record PlanLimitResponse
{
    public Guid SubscriptionPlanId { get; init; }
    public string Plan { get; init; }
    public string Limit { get; init; }
    public int Value { get; init; }
}
