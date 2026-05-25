namespace Domain.SubscriptionFeatures;

public static class DefaultPlanFeatures
{
    public static readonly HashSet<string> Free =
    [
    ];

    public static readonly HashSet<string> Pro =
    [
        SubscriptionFeature.ApiAccess,
        SubscriptionFeature.Reporting
    ];

    public static readonly HashSet<string> Enterprise =
    [
        ..SubscriptionFeature.All
    ];

    public static HashSet<string> GetDefaults(string planName) => planName switch
    {
        "Free" => Free,
        "Pro" => Pro,
        "Enterprise" => Enterprise,
        _ => []
    };
}
