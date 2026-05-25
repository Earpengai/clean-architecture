using Application.Abstractions.Messaging;

namespace Application.SubscriptionFeatures.UpdatePlanFeature;

public sealed record UpdatePlanFeatureCommand(
    Guid SubscriptionPlanId,
    string Feature,
    bool IsEnabled) : ICommand;
