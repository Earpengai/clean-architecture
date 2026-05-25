namespace Application.SubscriptionFeatures.GetPricing;

public sealed record PricingResponse
{
    public Guid PlanId { get; init; }
    public string Plan { get; init; }
    public string BillingPeriod { get; init; }
    public decimal Amount { get; init; }
}
