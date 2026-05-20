using Domain.Tenants;

namespace Application.Tenants.GetTenantById;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Identifier,
    SubscriptionPlan SubscriptionPlan,
    SubscriptionStatus SubscriptionStatus,
    int SeatCount);
