using Domain.SubscriptionFeatures;
using Domain.Tenants;

namespace Domain.UnitTests.SubscriptionFeatures;

public sealed class SubscriptionPricingTests
{
    [Fact]
    public void GetPrice_Free_ShouldReturnZero()
    {
        decimal price = SubscriptionPricing.GetPrice(SubscriptionPlan.Free, SubscriptionBillingPeriod.None);

        price.ShouldBe(0m);
    }

    [Fact]
    public void GetPrice_ProMonthly_ShouldReturnCorrectAmount()
    {
        decimal price = SubscriptionPricing.GetPrice(SubscriptionPlan.Pro, SubscriptionBillingPeriod.Monthly);

        price.ShouldBe(0.01m);
    }

    [Fact]
    public void GetPrice_EnterpriseYearly_ShouldReturnCorrectAmount()
    {
        decimal price = SubscriptionPricing.GetPrice(SubscriptionPlan.Enterprise, SubscriptionBillingPeriod.Yearly);

        price.ShouldBe(999.99m);
    }

    [Fact]
    public void Prices_Dictionary_ShouldHaveExpectedEntries()
    {
        SubscriptionPricing.Prices.Count.ShouldBe(5);
    }
}
