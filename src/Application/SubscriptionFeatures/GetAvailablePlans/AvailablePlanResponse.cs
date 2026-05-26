namespace Application.SubscriptionFeatures.GetAvailablePlans;

public sealed record AvailablePlanResponse(
    Guid PlanId,
    string Name,
    string? Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    int TrialDays,
    int RemainingQuota,
    HashSet<string> Features,
    Dictionary<string, int> Limits);