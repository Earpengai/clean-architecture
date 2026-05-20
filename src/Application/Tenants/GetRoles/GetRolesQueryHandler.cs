using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.GetRoles;

internal sealed class GetRolesQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetRolesQuery, List<RoleResponse>>
{
    public async Task<Result<List<RoleResponse>>> Handle(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        if (userContext.TenantId is null)
        {
            return Result.Failure<List<RoleResponse>>(UserErrors.Unauthorized());
        }

        List<RoleResponse> roles = await context.Roles
            .Where(r => r.TenantId == userContext.TenantId.Value)
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystem = r.IsSystem,
                Permissions = r.Permissions.Select(p => p.Permission).ToList()
            })
            .ToListAsync(cancellationToken);

        return roles;
    }
}
