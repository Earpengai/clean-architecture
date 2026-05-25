using Domain.SubscriptionFeatures;
using Domain.Tenants;

namespace Domain.UnitTests.SubscriptionFeatures;

public sealed class DefaultPlanLimitsTests
{
    [Fact]
    public void GetDefaults_Free_ShouldReturnCorrectLimits()
    {
        IReadOnlyDictionary<string, int> limits = DefaultPlanLimits.GetDefaults(SubscriptionPlan.Free);

        limits[SubscriptionLimit.MaxUsers].ShouldBe(3);
        limits[SubscriptionLimit.MaxTodos].ShouldBe(50);
        limits[SubscriptionLimit.StorageMb].ShouldBe(100);
    }

    [Fact]
    public void GetDefaults_Enterprise_AllLimitsShouldBeUnlimited()
    {
        IReadOnlyDictionary<string, int> limits = DefaultPlanLimits.GetDefaults(SubscriptionPlan.Enterprise);

        limits[SubscriptionLimit.MaxUsers].ShouldBe(SubscriptionLimit.Unlimited);
        limits[SubscriptionLimit.MaxTodos].ShouldBe(SubscriptionLimit.Unlimited);
        limits[SubscriptionLimit.StorageMb].ShouldBe(SubscriptionLimit.Unlimited);
    }
}
