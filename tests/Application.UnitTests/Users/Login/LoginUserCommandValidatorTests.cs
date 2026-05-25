using Application.Users.Login;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Login;

public sealed class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenEmailIsEmpty()
    {
        LoginUserCommand command = new("", "password");

        TestValidationResult<LoginUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenEmailIsInvalid()
    {
        LoginUserCommand command = new("invalid", "password");

        TestValidationResult<LoginUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenPasswordIsEmpty()
    {
        LoginUserCommand command = new("john@example.com", "");

        TestValidationResult<LoginUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAllFieldsValid()
    {
        LoginUserCommand command = new("john@example.com", "password");

        TestValidationResult<LoginUserCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
