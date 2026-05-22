namespace Application.SubscriptionFeatures.GetTenantFeatures;

public sealed record TenantFeaturesResponse
{
    public string SubscriptionPlan { get; init; }
    public string SubscriptionStatus { get; init; }
    public string BillingPeriod { get; init; }
    public string? SubscriptionExpiresAt { get; init; }
    public List<FeatureState> Features { get; init; } = [];
    public List<LimitState> Limits { get; init; } = [];
}

public sealed record FeatureState
{
    public string Feature { get; init; }
    public bool IsEnabled { get; init; }
}

public sealed record LimitState
{
    public string Limit { get; init; }
    public int Value { get; init; }
}
