using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Admin.UnlockUser;

internal sealed class UnlockUserCommandHandler(
    IUserContext userContext,
    UserManager<User> userManager)
    : ICommandHandler<UnlockUserCommand>
{
    public async Task<Result> Handle(UnlockUserCommand command, CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        User? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);

        return Result.Success();
    }
}
