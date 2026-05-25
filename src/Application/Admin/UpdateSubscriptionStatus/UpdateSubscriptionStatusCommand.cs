using Application.Abstractions.Messaging;
using Domain.Tenants;

namespace Application.Admin.UpdateSubscriptionStatus;

public sealed record UpdateSubscriptionStatusCommand(
    Guid SubscriptionId,
    SubscriptionStatus NewStatus) : ICommand;
