using SharedKernel;

namespace Domain.Tenants;

public static class TenantErrors
{
    public static readonly Error IdentifierNotUnique = Error.Conflict(
        "Tenants.IdentifierNotUnique",
        "The provided tenant identifier is already in use");

    public static Error NotFound(Guid tenantId) => Error.NotFound(
        "Tenants.NotFound",
        $"The tenant with the Id = '{tenantId}' was not found");

    public static Error NotFound(string identifier) => Error.NotFound(
        "Tenants.NotFound",
        $"The tenant with the identifier = '{identifier}' was not found");

    public static readonly Error FeatureNotFound = Error.NotFound(
        "Tenants.FeatureNotFound",
        "The specified subscription feature was not found");

    public static readonly Error LimitNotFound = Error.NotFound(
        "Tenants.LimitNotFound",
        "The specified subscription limit was not found");

    public static Error MaxFreeTenantsReached(Guid planId) => Error.Problem(
        "Tenants.MaxFreeTenantsReached",
        $"You have reached the maximum number of allowed tenants for the plan with Id = '{planId}'");

    public static readonly Error Disabled = Error.Problem(
        "Tenants.Disabled",
        "This tenant has been disabled. Please contact your administrator.");

    public static readonly Error SubscriptionExpired = Error.Problem(
        "Tenants.SubscriptionExpired",
        "Your subscription has expired. Please renew to regain access.");

    public static readonly Error TrialExpired = Error.Problem(
        "Tenants.TrialExpired",
        "Your trial period has ended. Please upgrade to a paid plan to continue.");

    public static Error MaxUsersReached(int limit) => Error.Problem(
        "Tenants.MaxUsersReached",
        $"This tenant has reached the maximum number of users ({limit}). Please upgrade your plan to add more users.");

    public static readonly Error DemoDataAlreadyCleared = Error.Conflict(
        "Tenants.DemoDataAlreadyCleared",
        "Demo data has already been cleared for this tenant. This operation can only be performed once.");

    public static readonly Error NotDemoTenant = Error.Problem(
        "Tenants.NotDemoTenant",
        "This tenant was not created with demo data.");

    public static readonly Error NotOwner = Error.Problem(
        "Tenants.NotOwner",
        "Only the tenant owner can perform this operation.");
}
