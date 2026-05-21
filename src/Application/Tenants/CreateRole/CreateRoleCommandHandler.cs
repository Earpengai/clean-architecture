using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Permissions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.CreateRole;

internal sealed class CreateRoleCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<CreateRoleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        bool nameExists = await context.Roles
            .AnyAsync(r => r.TenantId == userContext.TenantId!.Value
                && r.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(RoleErrors.NameNotUnique);
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = userContext.TenantId!.Value,
            Name = command.Name,
            Description = command.Description,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow
        };

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

        context.Roles.Add(role);

        await context.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
