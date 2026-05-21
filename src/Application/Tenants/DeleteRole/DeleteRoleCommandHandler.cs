using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.DeleteRole;

internal sealed class DeleteRoleCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        Role? role = await context.Roles
            .FirstOrDefaultAsync(r => r.Id == command.RoleId
                && r.TenantId == userContext.TenantId!.Value, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        if (role.IsSystem)
        {
            return Result.Failure(RoleErrors.CannotDeleteSystemRole);
        }

        bool hasMembers = await context.Memberships
            .AnyAsync(m => m.RoleId == command.RoleId, cancellationToken);

        if (hasMembers)
        {
            return Result.Failure(Error.Problem(
                "Roles.HasMembers",
                "Cannot delete a role that has members assigned to it"));
        }

        context.Roles.Remove(role);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
