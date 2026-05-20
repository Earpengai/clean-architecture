using Application.Abstractions.Messaging;
using Domain.Tenants;

namespace Application.Admin.UpdateTenantSubscription;

public sealed record UpdateTenantSubscriptionCommand(
    Guid TenantId,
    SubscriptionPlan SubscriptionPlan,
    SubscriptionStatus SubscriptionStatus,
    int SeatCount) : ICommand;
