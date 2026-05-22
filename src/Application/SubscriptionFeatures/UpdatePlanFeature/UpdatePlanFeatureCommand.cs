using Application.Abstractions.Messaging;
using Domain.Tenants;

namespace Application.SubscriptionFeatures.UpdatePlanFeature;

public sealed record UpdatePlanFeatureCommand(
    SubscriptionPlan Plan,
    string Feature,
    bool IsEnabled) : ICommand;
