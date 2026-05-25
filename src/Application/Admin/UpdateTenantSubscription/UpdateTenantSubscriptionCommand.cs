using Application.Abstractions.Messaging;
using Domain.Tenants;

namespace Application.Admin.UpdateTenantSubscription;

public sealed record UpdateTenantSubscriptionCommand(
    Guid TenantId,
    Guid SubscriptionPlanId,
    SubscriptionStatus SubscriptionStatus,
    int? MaxUsersOverride) : ICommand;
