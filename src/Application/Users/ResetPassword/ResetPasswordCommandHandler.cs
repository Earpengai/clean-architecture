using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.ResetPassword;

internal sealed class ResetPasswordCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.PasswordResetToken == command.Token, cancellationToken);

        if (user is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return Result.Failure(UserErrors.InvalidResetToken);
        }

        user.PasswordHash = passwordHasher.Hash(command.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
