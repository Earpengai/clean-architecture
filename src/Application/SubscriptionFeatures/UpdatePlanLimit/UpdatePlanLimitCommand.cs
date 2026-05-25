using Application.Abstractions.Messaging;

namespace Application.SubscriptionFeatures.UpdatePlanLimit;

public sealed record UpdatePlanLimitCommand(
    Guid SubscriptionPlanId,
    string Limit,
    int Value) : ICommand;
