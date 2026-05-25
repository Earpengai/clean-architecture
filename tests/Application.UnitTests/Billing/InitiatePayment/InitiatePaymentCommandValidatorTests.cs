using Application.Billing.InitiatePayment;
using Domain.Tenants;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Billing.InitiatePayment;

public sealed class InitiatePaymentCommandValidatorTests
{
    private readonly InitiatePaymentCommandValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenSubscriptionPlanIdIsEmpty()
    {
        var command = new InitiatePaymentCommand(Guid.Empty, SubscriptionBillingPeriod.Monthly);

        TestValidationResult<InitiatePaymentCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldNotHaveError_WhenSubscriptionPlanIdIsValid()
    {
        var command = new InitiatePaymentCommand(Guid.NewGuid(), SubscriptionBillingPeriod.Monthly);

        TestValidationResult<InitiatePaymentCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotHaveError_WhenBillingPeriodIsYearly()
    {
        var command = new InitiatePaymentCommand(Guid.NewGuid(), SubscriptionBillingPeriod.Yearly);

        TestValidationResult<InitiatePaymentCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }
}
