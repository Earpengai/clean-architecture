using Domain.Subscriptions;

namespace Domain.UnitTests.Subscriptions;

public sealed class SubscriptionPlanTests
{
    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CreatedAt = DateTime.UtcNow
        };

        plan.PriceMonthly.ShouldBe(0m);
        plan.PriceYearly.ShouldBe(0m);
        plan.TrialDays.ShouldBe(0);
        plan.SortOrder.ShouldBe(0);
        plan.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Pricing_ShouldStoreCorrectValues()
    {
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Pro",
            PriceMonthly = 29.99m,
            PriceYearly = 299.99m,
            TrialDays = 14,
            SortOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        plan.Name.ShouldBe("Pro");
        plan.PriceMonthly.ShouldBe(29.99m);
        plan.PriceYearly.ShouldBe(299.99m);
        plan.TrialDays.ShouldBe(14);
        plan.SortOrder.ShouldBe(1);
        plan.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void TrialDays_Zero_MeansNoExpiration()
    {
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Free",
            TrialDays = 0,
            CreatedAt = DateTime.UtcNow
        };

        plan.TrialDays.ShouldBe(0);
    }
}
