namespace Application.Admin.GetAllSubscriptions;

public sealed record SubscriptionResponse
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string TenantName { get; init; }
    public string PlanName { get; init; }
    public string Status { get; init; }
    public string BillingPeriod { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
