using Application.Abstractions.Messaging;

namespace Application.SubscriptionFeatures.GetPricing;

public sealed record GetPricingQuery : IQuery<List<PricingResponse>>;
