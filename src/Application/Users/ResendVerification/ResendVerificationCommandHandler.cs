using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.ResendVerification;

internal sealed class ResendVerificationCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager)
    : ICommandHandler<ResendVerificationCommand>
{
    public async Task<Result> Handle(ResendVerificationCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        user.Raise(new EmailVerificationRequestedDomainEvent(user.Id, user.Email!, token));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
