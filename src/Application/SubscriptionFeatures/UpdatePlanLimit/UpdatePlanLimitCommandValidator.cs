using Domain.SubscriptionFeatures;
using FluentValidation;

namespace Application.SubscriptionFeatures.UpdatePlanLimit;

internal sealed class UpdatePlanLimitCommandValidator : AbstractValidator<UpdatePlanLimitCommand>
{
    public UpdatePlanLimitCommandValidator()
    {
        RuleFor(c => c.Plan).IsInEnum();
        RuleFor(c => c.Limit).NotEmpty().Must(l => SubscriptionLimit.All.Contains(l))
            .WithMessage("The specified limit is not recognized.");
        RuleFor(c => c.Value)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("Value must be -1 (unlimited) or greater.");
    }
}
