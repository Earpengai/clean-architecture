using SharedKernel;

namespace Domain.Subscriptions;

public static class SubscriptionErrors
{
    public static Error NotFound(Guid subscriptionId) => Error.NotFound(
        "Subscriptions.NotFound",
        $"The subscription with the Id = '{subscriptionId}' was not found");

    public static Error PlanNotFound(Guid planId) => Error.NotFound(
        "Subscriptions.PlanNotFound",
        $"The subscription plan with the Id = '{planId}' was not found");

    public static readonly Error NoActivePlanAvailable = Error.NotFound(
        "Subscriptions.NoActivePlanAvailable",
        "No active subscription plan is available");

    public static Error PlanNameAlreadyExists(string name) => Error.Conflict(
        "Subscriptions.PlanNameAlreadyExists",
        $"A subscription plan with the name '{name}' already exists");
}
