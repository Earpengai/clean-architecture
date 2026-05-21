using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.VerifyEmail;

internal sealed class VerifyEmailCommandHandler(
    UserManager<User> userManager)
    : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.InvalidVerificationToken);
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        IdentityResult result = await userManager.ConfirmEmailAsync(user, command.Token);

        if (!result.Succeeded)
        {
            return Result.Failure(UserErrors.InvalidVerificationToken);
        }

        return Result.Success();
    }
}
