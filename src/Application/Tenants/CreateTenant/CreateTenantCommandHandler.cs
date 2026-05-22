using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Permissions;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider)
    : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    public async Task<Result<CreateTenantResponse>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        bool identifierExists = await context.Tenants
            .AnyAsync(t => t.Identifier == command.Identifier, cancellationToken);

        if (identifierExists)
        {
            return Result.Failure<CreateTenantResponse>(TenantErrors.IdentifierNotUnique);
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

        User? user = await context.Users.FirstOrDefaultAsync(
            u => u.Id == command.OwnerId, cancellationToken);

        List<string> tenantIdentifiers = await context.Memberships
            .Where(m => m.UserId == command.OwnerId)
            .Join(context.Tenants,
                m => m.TenantId,
                t => t.Id,
                (m, t) => t.Identifier)
            .ToListAsync(cancellationToken);

        string accessToken = tokenProvider.Create(
            user!, tenantIdentifiers, user!.IsSystemAdministrator);

        string plainRefreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        string refreshTokenHash = Convert.ToHexString(
            SHA256.HashData(Convert.FromHexString(plainRefreshToken)));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = command.OwnerId,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateTenantResponse(tenant.Id, accessToken, plainRefreshToken);
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
