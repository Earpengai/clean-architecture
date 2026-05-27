using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetUserSessions;

internal sealed class GetUserSessionsQueryHandler(
    IUserContext userContext,
    IApplicationDbContext context) : IQueryHandler<GetUserSessionsQuery, List<UserSessionResponse>>
{
    public async Task<Result<List<UserSessionResponse>>> Handle(
        GetUserSessionsQuery query,
        CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.UserId;

        List<UserSessionResponse> sessions = await context.UserSessions
            .Where(s => s.UserId == currentUserId && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt)
            .Select(s => new UserSessionResponse(
                s.Id,
                s.Browser,
                s.OperatingSystem,
                s.IpAddress,
                s.CreatedAt,
                s.LastActivityAt))
            .ToListAsync(cancellationToken);

        return sessions;
    }
}
