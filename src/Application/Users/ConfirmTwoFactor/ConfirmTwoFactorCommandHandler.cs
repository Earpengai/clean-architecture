using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.ConfirmTwoFactor;

internal sealed class ConfirmTwoFactorCommandHandler(
    IUserContext userContext,
    UserManager<User> userManager) : ICommandHandler<ConfirmTwoFactorCommand>
{
    public async Task<Result> Handle(ConfirmTwoFactorCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        bool isValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, command.Code);

        if (!isValid)
        {
            return Result.Failure(UserErrors.InvalidCredentials);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);

        return Result.Success();
    }
}
