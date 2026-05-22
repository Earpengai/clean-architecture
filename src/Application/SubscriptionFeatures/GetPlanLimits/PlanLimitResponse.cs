namespace Application.SubscriptionFeatures.GetPlanLimits;

public sealed record PlanLimitResponse
{
    public string Plan { get; init; }
    public string Limit { get; init; }
    public int Value { get; init; }
}
