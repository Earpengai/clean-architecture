using FluentValidation;

namespace Application.Admin.CreateSubscriptionPlan;

internal sealed class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Description).MaximumLength(500);
        RuleFor(c => c.PriceMonthly).GreaterThanOrEqualTo(0);
        RuleFor(c => c.PriceYearly).GreaterThanOrEqualTo(0);
        RuleFor(c => c.TrialDays).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
