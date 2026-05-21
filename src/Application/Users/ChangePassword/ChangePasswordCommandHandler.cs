using System.Linq;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    UserManager<User> userManager)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        IdentityResult result = await userManager.ChangePasswordAsync(
            user, command.CurrentPassword, command.NewPassword);

        if (!result.Succeeded)
        {
            IdentityError? firstError = result.Errors.FirstOrDefault();
            if (firstError is not null && string.Equals(firstError.Code, "PasswordMismatch", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(UserErrors.InvalidCredentials);
            }

            return Result.Failure(UserErrors.PasswordNotCompliant);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
