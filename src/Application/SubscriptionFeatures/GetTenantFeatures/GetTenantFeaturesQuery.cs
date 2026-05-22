using Application.Abstractions.Messaging;

namespace Application.SubscriptionFeatures.GetTenantFeatures;

public sealed record GetTenantFeaturesQuery : IQuery<TenantFeaturesResponse>;
