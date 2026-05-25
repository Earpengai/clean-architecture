using Domain.SubscriptionFeatures;

namespace Domain.UnitTests.SubscriptionFeatures;

public sealed class DefaultPlanLimitsTests
{
    [Fact]
    public void GetDefaults_Free_ShouldReturnCorrectLimits()
    {
        IReadOnlyDictionary<string, int> limits = DefaultPlanLimits.GetDefaults("Free");

        limits[SubscriptionLimit.MaxUsers].ShouldBe(3);
        limits[SubscriptionLimit.MaxTodos].ShouldBe(50);
        limits[SubscriptionLimit.StorageMb].ShouldBe(100);
        limits[SubscriptionLimit.MaxTenantsPerUser].ShouldBe(1);
    }

    [Fact]
    public void GetDefaults_Enterprise_AllLimitsShouldBeUnlimited()
    {
        IReadOnlyDictionary<string, int> limits = DefaultPlanLimits.GetDefaults("Enterprise");

        limits[SubscriptionLimit.MaxUsers].ShouldBe(SubscriptionLimit.Unlimited);
        limits[SubscriptionLimit.MaxTodos].ShouldBe(SubscriptionLimit.Unlimited);
        limits[SubscriptionLimit.StorageMb].ShouldBe(SubscriptionLimit.Unlimited);
        limits[SubscriptionLimit.MaxTenantsPerUser].ShouldBe(SubscriptionLimit.Unlimited);
    }
}
