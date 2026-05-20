using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.UpdateProfile;

internal sealed class UpdateUserProfileCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateUserProfileCommand>
{
    public async Task<Result> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
