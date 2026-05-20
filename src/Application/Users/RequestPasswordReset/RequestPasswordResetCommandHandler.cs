using System.Security.Cryptography;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.RequestPasswordReset;

internal sealed class RequestPasswordResetCommandHandler(IApplicationDbContext context)
    : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<Result> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Success();
        }

        user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        user.Raise(new PasswordResetRequestedDomainEvent(user.Id, user.Email));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
