using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetTenantUserById;

internal sealed class GetTenantUserByIdQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTenantUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(
        GetTenantUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        Domain.Tenants.Membership? membership = await context.Memberships
            .Include(m => m.Role)
            .Include(m => m.User)
            .FirstOrDefaultAsync(m =>
                m.UserId == query.UserId && m.TenantId == userContext.TenantId!.Value,
                cancellationToken);

        if (membership is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
        }

        return new UserResponse
        {
            Id = membership.User.Id,
            Email = membership.User.Email!,
            FirstName = membership.User.FirstName,
            LastName = membership.User.LastName,
            EmailConfirmed = membership.User.EmailConfirmed,
            CreatedAt = membership.User.CreatedAt,
            RoleName = membership.Role.Name!,
            RoleId = membership.Role.Id
        };
    }
}
