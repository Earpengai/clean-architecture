using FluentValidation;

namespace Application.Users.ResendVerification;

internal sealed class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
    }
}
