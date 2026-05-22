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
}
