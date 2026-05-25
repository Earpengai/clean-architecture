using FluentValidation;

namespace Application.Billing.InitiatePayment;

internal sealed class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(c => c.SubscriptionPlanId).NotEmpty();
        RuleFor(c => c.BillingPeriod).IsInEnum();
    }
}
