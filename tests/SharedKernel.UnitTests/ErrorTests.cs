using SharedKernel;

namespace SharedKernel.UnitTests;

public sealed class ErrorTests
{
    [Fact]
    public void Failure_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Failure("Code", "Description");

        error.Code.ShouldBe("Code");
        error.Description.ShouldBe("Description");
        error.Type.ShouldBe(ErrorType.Failure);
    }

    [Fact]
    public void NotFound_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.NotFound("Code", "Description");

        error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public void Problem_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Problem("Code", "Description");

        error.Type.ShouldBe(ErrorType.Problem);
    }

    [Fact]
    public void Conflict_ShouldCreateErrorWithCorrectType()
    {
        var error = Error.Conflict("Code", "Description");

        error.Type.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public void None_ShouldHaveEmptyCodeAndDescription()
    {
        Error.None.Code.ShouldBe(string.Empty);
        Error.None.Description.ShouldBe(string.Empty);
    }

    [Fact]
    public void NullValue_ShouldHaveFailureType()
    {
        Error.NullValue.Type.ShouldBe(ErrorType.Failure);
        Error.NullValue.Code.ShouldBe("General.Null");
    }

    [Fact]
    public void Error_ShouldBeEqualWhenSameValues()
    {
        var error1 = Error.Failure("A", "B");
        var error2 = Error.Failure("A", "B");

        error1.ShouldBe(error2);
    }
}
