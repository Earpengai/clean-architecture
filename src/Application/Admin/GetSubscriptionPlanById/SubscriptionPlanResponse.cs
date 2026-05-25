namespace Application.Admin.GetSubscriptionPlanById;

public sealed record SubscriptionPlanResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public decimal PriceMonthly { get; init; }
    public decimal PriceYearly { get; init; }
    public int TrialDays { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<PlanFeatureInfo> Features { get; init; } = [];
    public List<PlanLimitInfo> Limits { get; init; } = [];
}

public sealed record PlanFeatureInfo
{
    public string Feature { get; init; }
    public bool IsEnabled { get; init; }
}

public sealed record PlanLimitInfo
{
    public string Limit { get; init; }
    public int Value { get; init; }
}
