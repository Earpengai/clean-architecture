using Domain.SubscriptionFeatures;
using FluentValidation;

namespace Application.SubscriptionFeatures.UpdatePlanFeature;

internal sealed class UpdatePlanFeatureCommandValidator : AbstractValidator<UpdatePlanFeatureCommand>
{
    public UpdatePlanFeatureCommandValidator()
    {
        RuleFor(c => c.SubscriptionPlanId).NotEmpty();
        RuleFor(c => c.Feature).NotEmpty().Must(f => SubscriptionFeature.All.Contains(f))
            .WithMessage("The specified feature is not recognized.");
    }
}
