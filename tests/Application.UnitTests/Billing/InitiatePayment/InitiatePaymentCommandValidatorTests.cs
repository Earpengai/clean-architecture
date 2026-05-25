using Application.Billing.InitiatePayment;
using Domain.Tenants;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Billing.InitiatePayment;

public sealed class InitiatePaymentCommandValidatorTests
{
    private readonly InitiatePaymentCommandValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenPlanIsFree()
    {
        var command = new InitiatePaymentCommand(SubscriptionPlan.Free, SubscriptionBillingPeriod.Monthly);

        TestValidationResult<InitiatePaymentCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldNotHaveError_WhenPlanIsPro()
    {
        var command = new InitiatePaymentCommand(SubscriptionPlan.Pro, SubscriptionBillingPeriod.Monthly);

        TestValidationResult<InitiatePaymentCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotHaveError_WhenPlanIsEnterprise()
    {
        var command = new InitiatePaymentCommand(SubscriptionPlan.Enterprise, SubscriptionBillingPeriod.Yearly);

        TestValidationResult<InitiatePaymentCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }
}
