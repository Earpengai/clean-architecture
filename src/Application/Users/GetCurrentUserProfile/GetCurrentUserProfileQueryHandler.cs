using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetCurrentUserProfile;

internal sealed class GetCurrentUserProfileQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetCurrentUserProfileQuery, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(
        GetCurrentUserProfileQuery query,
        CancellationToken cancellationToken)
    {
        UserProfileResponse? user = await context.Users
            .Where(u => u.Id == userContext.UserId)
            .Select(u => new UserProfileResponse
            {
                Id = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                TwoFactorEnabled = u.TwoFactorEnabled,
                EmailConfirmed = u.EmailConfirmed
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserProfileResponse>(UserErrors.NotFound(userContext.UserId));
        }

        return user;
    }
}
