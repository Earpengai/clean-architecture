using Domain.SubscriptionFeatures;

namespace Domain.UnitTests.SubscriptionFeatures;

public sealed class DefaultPlanFeaturesTests
{
    [Fact]
    public void GetDefaults_Free_ShouldReturnEmptySet()
    {
        HashSet<string> features = DefaultPlanFeatures.GetDefaults("Free");

        features.Count.ShouldBe(0);
    }

    [Fact]
    public void GetDefaults_Pro_ShouldContainApiAccessAndReporting()
    {
        HashSet<string> features = DefaultPlanFeatures.GetDefaults("Pro");

        features.ShouldContain(SubscriptionFeature.ApiAccess);
        features.ShouldContain(SubscriptionFeature.Reporting);
        features.Count.ShouldBe(2);
    }

    [Fact]
    public void GetDefaults_Enterprise_ShouldContainAllFeatures()
    {
        HashSet<string> features = DefaultPlanFeatures.GetDefaults("Enterprise");

        features.SetEquals(SubscriptionFeature.All).ShouldBeTrue();
    }
}
