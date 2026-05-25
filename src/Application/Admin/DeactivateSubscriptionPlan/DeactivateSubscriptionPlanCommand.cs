using Application.Abstractions.Messaging;

namespace Application.Admin.DeactivateSubscriptionPlan;

public sealed record DeactivateSubscriptionPlanCommand(Guid SubscriptionPlanId) : ICommand;
