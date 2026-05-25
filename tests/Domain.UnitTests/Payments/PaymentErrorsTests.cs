using Domain.Payments;
using SharedKernel;

namespace Domain.UnitTests.Payments;

public sealed class PaymentErrorsTests
{
    [Fact]
    public void NotFound_ShouldReturnErrorWithId()
    {
        var paymentId = Guid.NewGuid();

        Error error = PaymentErrors.NotFound(paymentId);

        error.Type.ShouldBe(ErrorType.NotFound);
        error.Code.ShouldBe("Payments.NotFound");
        error.Description.ShouldContain(paymentId.ToString());
    }

    [Fact]
    public void NotOwner_ShouldBeFailureType()
    {
        PaymentErrors.NotOwner.Type.ShouldBe(ErrorType.Failure);
    }

    [Fact]
    public void AlreadyCompleted_ShouldBeConflictType()
    {
        PaymentErrors.AlreadyCompleted.Type.ShouldBe(ErrorType.Conflict);
    }
}
