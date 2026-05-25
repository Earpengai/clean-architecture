using Domain.Tenants;

namespace Application.Tenants.GetTenantById;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Identifier,
    string? SubscriptionPlanName,
    SubscriptionStatus? SubscriptionStatus,
    int? MaxUsersOverride,
    string? Role,
    string? BillingPeriod,
    DateTime? SubscriptionExpiresAt);
