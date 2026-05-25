using SharedKernel;

namespace SharedKernel.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldIndicateSuccess()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_ShouldContainError()
    {
        var error = Error.Failure("Test.Error", "Test error description");

        var result = Result.Failure(error);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void SuccessT_ShouldHaveValue()
    {
        string value = "test-value";

        var result = Result.Success(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void FailureT_ShouldThrowOnValueAccess()
    {
        var result = Result.Failure<string>(Error.Failure("E", "desc"));

        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertNonNullValue()
    {
        Result<string> result = "hello";

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertNullValueToFailure()
    {
        string? nullString = null;

        Result<string> result = nullString;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Error.NullValue);
    }

    [Fact]
    public void ValidationFailureT_ShouldCreateFailureResult()
    {
        var error = Error.Problem("Code", "Description");

        var result = Result<string>.ValidationFailure(error);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }
}
