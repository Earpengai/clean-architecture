using Application.Abstractions.Messaging;

namespace Application.Admin.CreateSubscriptionPlan;

public sealed record CreateSubscriptionPlanCommand(
    string Name,
    string? Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    int TrialDays,
    int SortOrder,
    bool IsActive) : ICommand<Guid>;
