using Application.Abstractions.Messaging;

namespace Application.SubscriptionFeatures.GetPlanFeatures;

public sealed record GetPlanFeaturesQuery : IQuery<List<PlanFeatureResponse>>;
