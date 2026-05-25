using Application.Abstractions.Messaging;

namespace Application.Admin.UpdateSubscriptionPlan;

public sealed record UpdateSubscriptionPlanCommand(
    Guid SubscriptionPlanId,
    string Name,
    string? Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    int TrialDays,
    int SortOrder,
    bool IsActive) : ICommand;
