using Domain.Tenants;
using FluentValidation;

namespace Application.Billing.InitiatePayment;

internal sealed class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(c => c.Plan).IsInEnum();
        RuleFor(c => c.BillingPeriod).IsInEnum();
        RuleFor(c => c)
            .Must(c => c.Plan != SubscriptionPlan.Free)
            .WithMessage("Cannot initiate payment for the Free plan.");
    }
}
