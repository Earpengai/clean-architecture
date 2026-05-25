using Application.Abstractions.Messaging;

namespace Application.Admin.GetSubscriptionPlanById;

public sealed record GetSubscriptionPlanByIdQuery(Guid SubscriptionPlanId) : IQuery<SubscriptionPlanDetail>;
