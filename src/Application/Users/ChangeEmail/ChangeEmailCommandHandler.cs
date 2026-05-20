using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.ChangeEmail;

internal sealed class ChangeEmailCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<ChangeEmailCommand>
{
    public async Task<Result> Handle(ChangeEmailCommand command, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.NewEmail, cancellationToken))
        {
            return Result.Failure(UserErrors.EmailNotUnique);
        }

        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        user.Email = command.NewEmail;
        user.EmailVerified = false;
        user.EmailVerificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        user.Raise(new EmailVerificationRequestedDomainEvent(user.Id, user.Email));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
