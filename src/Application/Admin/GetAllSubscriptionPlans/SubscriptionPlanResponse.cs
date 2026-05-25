namespace Application.Admin.GetAllSubscriptionPlans;

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
    public int SubscriptionCount { get; init; }
}
