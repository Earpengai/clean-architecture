namespace Application.Admin.GetTenantSubscription;

public sealed record TenantSubscriptionResponse
{
    public Guid SubscriptionId { get; init; }
    public Guid TenantId { get; init; }
    public string TenantName { get; init; }
    public string PlanName { get; init; }
    public string Status { get; init; }
    public string BillingPeriod { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
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
