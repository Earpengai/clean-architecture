using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IPasswordHasher passwordHasher)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        bool verified = passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);

        if (!verified)
        {
            return Result.Failure(UserErrors.InvalidCredentials);
        }

        user.PasswordHash = passwordHasher.Hash(command.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
