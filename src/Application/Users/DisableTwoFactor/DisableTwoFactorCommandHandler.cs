using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.DisableTwoFactor;

internal sealed class DisableTwoFactorCommandHandler(
    IUserContext userContext,
    UserManager<User> userManager) : ICommandHandler<DisableTwoFactorCommand>
{
    public async Task<Result> Handle(DisableTwoFactorCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);

        return Result.Success();
    }
}
