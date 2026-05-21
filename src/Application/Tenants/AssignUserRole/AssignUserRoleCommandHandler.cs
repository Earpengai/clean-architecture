using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.AssignUserRole;

internal sealed class AssignUserRoleCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<AssignUserRoleCommand>
{
    public async Task<Result> Handle(AssignUserRoleCommand command, CancellationToken cancellationToken)
    {
        Membership? membership = await context.Memberships
            .FirstOrDefaultAsync(m => m.UserId == command.UserId
                && m.TenantId == userContext.TenantId!.Value, cancellationToken);

        if (membership is null)
        {
            return Result.Failure(UserErrors.NotMemberOfTenant);
        }

        Role? role = await context.Roles
            .FirstOrDefaultAsync(r => r.Id == command.RoleId
                && r.TenantId == userContext.TenantId!.Value, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        membership.RoleId = command.RoleId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
