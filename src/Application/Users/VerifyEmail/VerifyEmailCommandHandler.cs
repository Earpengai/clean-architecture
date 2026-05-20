using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.VerifyEmail;

internal sealed class VerifyEmailCommandHandler(IApplicationDbContext context)
    : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.EmailVerificationToken == command.Token, cancellationToken);

        if (user is null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return Result.Failure(UserErrors.InvalidVerificationToken);
        }

        if (user.EmailVerified)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
