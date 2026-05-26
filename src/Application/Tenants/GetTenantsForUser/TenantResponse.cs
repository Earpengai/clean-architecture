using Domain.Tenants;

namespace Application.Tenants.GetTenantsForUser;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Identifier,
    string? SubscriptionPlanName,
    SubscriptionStatus? SubscriptionStatus,
    int? MaxUsersOverride,
    string Role,
    bool IsActive,
    DateTime? SubscriptionExpiresAt);
