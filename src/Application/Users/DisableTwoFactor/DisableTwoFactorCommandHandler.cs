using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.DisableTwoFactor;

internal sealed class DisableTwoFactorCommandHandler(
    IUserContext userContext,
    IApplicationDbContext context,
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

        List<TwoFactorRememberToken> tokens = await context.TwoFactorRememberTokens
            .Where(t => t.UserId == user.Id)
            .ToListAsync(cancellationToken);

        context.TwoFactorRememberTokens.RemoveRange(tokens);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
