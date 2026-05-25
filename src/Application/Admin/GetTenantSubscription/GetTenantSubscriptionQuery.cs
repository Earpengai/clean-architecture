using Application.Abstractions.Messaging;

namespace Application.Admin.GetTenantSubscription;

public sealed record GetTenantSubscriptionQuery(Guid TenantId) : IQuery<TenantSubscriptionResponse>;
