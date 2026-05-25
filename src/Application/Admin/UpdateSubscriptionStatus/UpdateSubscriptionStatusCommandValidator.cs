using FluentValidation;

namespace Application.Admin.UpdateSubscriptionStatus;

internal sealed class UpdateSubscriptionStatusCommandValidator : AbstractValidator<UpdateSubscriptionStatusCommand>
{
    public UpdateSubscriptionStatusCommandValidator()
    {
        RuleFor(c => c.SubscriptionId).NotEmpty();
        RuleFor(c => c.NewStatus).IsInEnum();
    }
}
