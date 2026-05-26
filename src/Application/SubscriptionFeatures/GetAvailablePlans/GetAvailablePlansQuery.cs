using Application.Abstractions.Messaging;

namespace Application.SubscriptionFeatures.GetAvailablePlans;

public sealed record GetAvailablePlansQuery : IQuery<List<AvailablePlanResponse>>;