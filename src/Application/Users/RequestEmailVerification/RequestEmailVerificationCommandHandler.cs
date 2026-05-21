using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.RequestEmailVerification;

internal sealed class RequestEmailVerificationCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    UserManager<User> userManager)
    : ICommandHandler<RequestEmailVerificationCommand>
{
    public async Task<Result> Handle(RequestEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
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
