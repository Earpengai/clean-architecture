using FluentValidation;

namespace Application.Users.RevokeSession;

internal sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
    }
}
