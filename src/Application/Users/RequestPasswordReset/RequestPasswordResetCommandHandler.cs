using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.RequestPasswordReset;

internal sealed class RequestPasswordResetCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager)
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

        string token = await userManager.GeneratePasswordResetTokenAsync(user);

        user.Raise(new PasswordResetRequestedDomainEvent(user.Id, user.Email!, token));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
