namespace Domain.SubscriptionFeatures;

public static class DefaultPlanLimits
{
    public static IReadOnlyDictionary<string, int> Free { get; } = new Dictionary<string, int>
    {
        [SubscriptionLimit.MaxUsers] = 3,
        [SubscriptionLimit.MaxTodos] = 50,
        [SubscriptionLimit.StorageMb] = 100,
        [SubscriptionLimit.MaxTenantsPerUser] = 1
    };

    public static IReadOnlyDictionary<string, int> Pro { get; } = new Dictionary<string, int>
    {
        [SubscriptionLimit.MaxUsers] = 20,
        [SubscriptionLimit.MaxTodos] = 500,
        [SubscriptionLimit.StorageMb] = 1024,
        [SubscriptionLimit.MaxTenantsPerUser] = SubscriptionLimit.Unlimited
    };

    public static IReadOnlyDictionary<string, int> Enterprise { get; } = new Dictionary<string, int>
    {
        [SubscriptionLimit.MaxUsers] = SubscriptionLimit.Unlimited,
        [SubscriptionLimit.MaxTodos] = SubscriptionLimit.Unlimited,
        [SubscriptionLimit.StorageMb] = SubscriptionLimit.Unlimited,
        [SubscriptionLimit.MaxTenantsPerUser] = SubscriptionLimit.Unlimited
    };

    public static IReadOnlyDictionary<string, int> GetDefaults(string planName) => planName switch
    {
        "Free" => Free,
        "Pro" => Pro,
        "Enterprise" => Enterprise,
        _ => new Dictionary<string, int>()
    };
}
