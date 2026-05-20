using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.RequestEmailVerification;

internal sealed class RequestEmailVerificationCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
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

        if (user.EmailVerified)
        {
            return Result.Failure(UserErrors.EmailAlreadyVerified);
        }

        user.EmailVerificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        user.Raise(new EmailVerificationRequestedDomainEvent(user.Id, user.Email));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
