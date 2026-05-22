using Application.Abstractions.Messaging;
using Domain.Tenants;

namespace Application.SubscriptionFeatures.UpdatePlanLimit;

public sealed record UpdatePlanLimitCommand(
    SubscriptionPlan Plan,
    string Limit,
    int Value) : ICommand;
