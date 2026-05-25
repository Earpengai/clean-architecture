using Domain.SubscriptionFeatures;
using Domain.Tenants;

namespace Domain.UnitTests.SubscriptionFeatures;

public sealed class DefaultPlanFeaturesTests
{
    [Fact]
    public void GetDefaults_Free_ShouldReturnEmptySet()
    {
        HashSet<string> features = DefaultPlanFeatures.GetDefaults(SubscriptionPlan.Free);

        features.Count.ShouldBe(0);
    }

    [Fact]
    public void GetDefaults_Pro_ShouldContainApiAccessAndReporting()
    {
        HashSet<string> features = DefaultPlanFeatures.GetDefaults(SubscriptionPlan.Pro);

        features.ShouldContain(SubscriptionFeature.ApiAccess);
        features.ShouldContain(SubscriptionFeature.Reporting);
        features.Count.ShouldBe(2);
    }

    [Fact]
    public void GetDefaults_Enterprise_ShouldContainAllFeatures()
    {
        HashSet<string> features = DefaultPlanFeatures.GetDefaults(SubscriptionPlan.Enterprise);

        features.SetEquals(SubscriptionFeature.All).ShouldBeTrue();
    }
}
