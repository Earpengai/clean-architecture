namespace Application.SubscriptionFeatures.GetPricing;

public sealed record PricingResponse
{
    public string Plan { get; init; }
    public string BillingPeriod { get; init; }
    public decimal Amount { get; init; }
}
