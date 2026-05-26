using Application.Users.Register;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Register;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenEmailIsEmpty()
    {
        RegisterUserCommand command = new("", "John", "Doe", "StrongP@ss1", "StrongP@ss1", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenEmailIsInvalid()
    {
        RegisterUserCommand command = new("not-an-email", "John", "Doe", "StrongP@ss1", "StrongP@ss1", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void ShouldHaveError_WhenFirstNameIsEmpty()
    {
        RegisterUserCommand command = new("john@example.com", "", "Doe", "StrongP@ss1", "StrongP@ss1", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void ShouldHaveError_WhenLastNameIsEmpty()
    {
        RegisterUserCommand command = new("john@example.com", "John", "", "StrongP@ss1", "StrongP@ss1", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public void ShouldHaveError_WhenPasswordIsTooShort()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "Sh0r!", "Sh0r!", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void ShouldHaveError_WhenPasswordLacksUppercase()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "alllowercase1!", "alllowercase1!", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void ShouldHaveError_WhenPasswordLacksDigit()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "NoDigitHere!", "NoDigitHere!", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void ShouldHaveError_WhenConfirmPasswordDoesNotMatch()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "StrongP@ss1", "MismatchP@ss1", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ConfirmPassword);
    }

    [Fact]
    public void ShouldHaveError_WhenCompanyNameIsEmpty()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "StrongP@ss1", "StrongP@ss1", "", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CompanyName);
    }

    [Fact]
    public void ShouldHaveError_WhenIndustryIsEmpty()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "StrongP@ss1", "StrongP@ss1", "Acme", "", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Industry);
    }

    [Fact]
    public void ShouldHaveError_WhenCountryIsEmpty()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "StrongP@ss1", "StrongP@ss1", "Acme", "Tech", "", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Country);
    }

    [Fact]
    public void ShouldHaveError_WhenAcceptedTermsIsFalse()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "StrongP@ss1", "StrongP@ss1", "Acme", "Tech", "KH", false);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AcceptedTerms);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAllFieldsValid()
    {
        RegisterUserCommand command = new("john@example.com", "John", "Doe", "StrongP@ss1", "StrongP@ss1", "Acme", "Tech", "KH", true);

        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
