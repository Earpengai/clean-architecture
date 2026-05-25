namespace Application.SubscriptionFeatures.GetPlanFeatures;

public sealed record PlanFeatureResponse
{
    public Guid SubscriptionPlanId { get; init; }
    public string Plan { get; init; }
    public string Feature { get; init; }
    public bool IsEnabled { get; init; }
}
