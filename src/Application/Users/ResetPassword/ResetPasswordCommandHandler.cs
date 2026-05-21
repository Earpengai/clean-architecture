using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.ResetPassword;

internal sealed class ResetPasswordCommandHandler(
    UserManager<User> userManager)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.InvalidResetToken);
        }

        IdentityResult result = await userManager.ResetPasswordAsync(
            user, command.Token, command.NewPassword);

        if (!result.Succeeded)
        {
            return Result.Failure(UserErrors.InvalidResetToken);
        }

        return Result.Success();
    }
}
