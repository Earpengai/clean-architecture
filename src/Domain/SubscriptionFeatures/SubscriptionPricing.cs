using Domain.Tenants;

namespace Domain.SubscriptionFeatures;

public static class SubscriptionPricing
{
    public static IReadOnlyDictionary<(SubscriptionPlan Plan, SubscriptionBillingPeriod Period), decimal> Prices { get; } =
        new Dictionary<(SubscriptionPlan, SubscriptionBillingPeriod), decimal>
        {
            [(SubscriptionPlan.Free, SubscriptionBillingPeriod.None)] = 0m,
            [(SubscriptionPlan.Pro, SubscriptionBillingPeriod.Monthly)] = 0.01m,
            [(SubscriptionPlan.Pro, SubscriptionBillingPeriod.Yearly)] = 299.99m,
            [(SubscriptionPlan.Enterprise, SubscriptionBillingPeriod.Monthly)] = 99.99m,
            [(SubscriptionPlan.Enterprise, SubscriptionBillingPeriod.Yearly)] = 999.99m,
        };

    public static decimal GetPrice(SubscriptionPlan plan, SubscriptionBillingPeriod period)
    {
        return Prices.TryGetValue((plan, period), out decimal price) ? price : 0m;
    }
}
