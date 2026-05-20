using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Permissions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTenantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        bool identifierExists = await context.Tenants
            .AnyAsync(t => t.Identifier == command.Identifier, cancellationToken);

        if (identifierExists)
        {
            return Result.Failure<Guid>(TenantErrors.IdentifierNotUnique);
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Identifier = command.Identifier,
            SubscriptionPlan = SubscriptionPlan.Free,
            SubscriptionStatus = SubscriptionStatus.Trialing,
            SeatCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        Role ownerRole = CreateSystemRole(tenant.Id, "Owner", DefaultPermissions.Owner);
        Role adminRole = CreateSystemRole(tenant.Id, "Admin", DefaultPermissions.Admin);
        Role memberRole = CreateSystemRole(tenant.Id, "Member", DefaultPermissions.Member);

        var membership = new Membership
        {
            UserId = command.OwnerId,
            TenantId = tenant.Id,
            RoleId = ownerRole.Id,
            JoinedAt = DateTime.UtcNow
        };

        context.Tenants.Add(tenant);
        context.Roles.Add(ownerRole);
        context.Roles.Add(adminRole);
        context.Roles.Add(memberRole);
        context.Memberships.Add(membership);

        await context.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }

    private static Role CreateSystemRole(Guid tenantId, string name, HashSet<string> permissions)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            IsSystem = true,
            CreatedAt = DateTime.UtcNow,
            Permissions = []
        };

        foreach (string permission in permissions)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission
            });
        }

        return role;
    }
}
