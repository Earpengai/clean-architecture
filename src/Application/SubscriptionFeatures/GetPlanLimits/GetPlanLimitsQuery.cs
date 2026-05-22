using Application.Abstractions.Messaging;

namespace Application.SubscriptionFeatures.GetPlanLimits;

public sealed record GetPlanLimitsQuery : IQuery<List<PlanLimitResponse>>;
