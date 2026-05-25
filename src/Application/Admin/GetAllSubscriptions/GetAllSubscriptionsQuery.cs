using Application.Abstractions.Messaging;

namespace Application.Admin.GetAllSubscriptions;

public sealed record GetAllSubscriptionsQuery : IQuery<List<SubscriptionResponse>>;
