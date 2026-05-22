using Domain.Tenants;

namespace Domain.SubscriptionFeatures;

public static class DefaultPlanLimits
{
    public static IReadOnlyDictionary<string, int> Free { get; } = new Dictionary<string, int>
    {
        [SubscriptionLimit.MaxUsers] = 3,
        [SubscriptionLimit.MaxTodos] = 50,
        [SubscriptionLimit.StorageMb] = 100
    };

    public static IReadOnlyDictionary<string, int> Pro { get; } = new Dictionary<string, int>
    {
        [SubscriptionLimit.MaxUsers] = 20,
        [SubscriptionLimit.MaxTodos] = 500,
        [SubscriptionLimit.StorageMb] = 1024
    };

    public static IReadOnlyDictionary<string, int> Enterprise { get; } = new Dictionary<string, int>
    {
        [SubscriptionLimit.MaxUsers] = SubscriptionLimit.Unlimited,
        [SubscriptionLimit.MaxTodos] = SubscriptionLimit.Unlimited,
        [SubscriptionLimit.StorageMb] = SubscriptionLimit.Unlimited
    };

    public static IReadOnlyDictionary<string, int> GetDefaults(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Free => Free,
        SubscriptionPlan.Pro => Pro,
        SubscriptionPlan.Enterprise => Enterprise,
        _ => new Dictionary<string, int>()
    };
}
