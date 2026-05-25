using Domain.Tenants;

namespace Application.Tenants.GetTenantsForUser;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Identifier,
    string? SubscriptionPlanName,
    SubscriptionStatus? SubscriptionStatus,
    int SeatCount,
    string Role);
