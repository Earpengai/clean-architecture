using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetTenantUsers;

internal sealed class GetTenantUsersQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTenantUsersQuery, List<UserResponse>>
{
    public async Task<Result<List<UserResponse>>> Handle(
        GetTenantUsersQuery query,
        CancellationToken cancellationToken)
    {
        List<UserResponse> users = await context.Memberships
            .Join(context.Users,
                m => m.UserId,
                u => u.Id,
                (m, u) => new { Membership = m, User = u })
            .Join(context.Roles,
                x => x.Membership.RoleId,
                r => r.Id,
                (x, r) => new UserResponse
                {
                    Id = x.User.Id,
                    Email = x.User.Email!,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    EmailConfirmed = x.User.EmailConfirmed,
                    CreatedAt = x.User.CreatedAt,
                    RoleName = r.Name!,
                    RoleId = r.Id
                })
            .ToListAsync(cancellationToken);

        return users;
    }
}
