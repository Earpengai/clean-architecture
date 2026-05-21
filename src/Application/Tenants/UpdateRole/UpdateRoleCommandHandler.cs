using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Permissions;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.UpdateRole;

internal sealed class UpdateRoleCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        Role? role = await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == command.RoleId
                && r.TenantId == userContext.TenantId!.Value, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        if (role.IsSystem)
        {
            return Result.Failure(RoleErrors.CannotModifySystemRole);
        }

        bool nameExists = await context.Roles
            .AnyAsync(r => r.TenantId == userContext.TenantId!.Value
                && r.Name == command.Name && r.Id != command.RoleId, cancellationToken);

        if (nameExists)
        {
            return Result.Failure(RoleErrors.NameNotUnique);
        }

        role.Name = command.Name;
        role.Description = command.Description;

        role.Permissions.Clear();

        foreach (string permission in command.Permissions)
        {
            if (Permission.All.Contains(permission))
            {
                role.Permissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    Permission = permission
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
