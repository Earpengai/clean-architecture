using Application.Abstractions.Messaging;

namespace Application.Admin.GetAllSubscriptionPlans;

public sealed record GetAllSubscriptionPlansQuery : IQuery<List<SubscriptionPlanListItem>>;
