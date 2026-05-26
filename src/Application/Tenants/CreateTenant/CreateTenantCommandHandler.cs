using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Jobs;
using Application.Abstractions.Messaging;
using Application.Tenants.SeedDemoData;
using Domain.Permissions;
using Domain.SubscriptionFeatures;
using Domain.Subscriptions;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider,
    IBackgroundJobQueue jobQueue)
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

        SubscriptionPlan? plan = command.SubscriptionPlanId is not null
            ? await context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == command.SubscriptionPlanId.Value, cancellationToken)
            : await context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .FirstOrDefaultAsync(cancellationToken);

        if (plan is null)
        {
            return Result.Failure<CreateTenantResponse>(SubscriptionErrors.NoActivePlanAvailable);
        }

        int? limitValue = await context.PlanLimits
            .Where(pl => pl.SubscriptionPlanId == plan.Id && pl.Limit == SubscriptionLimit.MaxTenantsPerUser)
            .Select(pl => (int?)pl.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (limitValue is not null && limitValue != SubscriptionLimit.Unlimited)
        {
            List<Guid> userTenantIds = await context.Memberships
                .Where(m => m.UserId == command.OwnerId && m.Role.Name == "Owner")
                .Select(m => m.TenantId)
                .ToListAsync(cancellationToken);

            int ownedCount = await context.Subscriptions
                .Where(s => userTenantIds.Contains(s.TenantId) && s.SubscriptionPlanId == plan.Id)
                .CountAsync(cancellationToken);

            if (ownedCount >= limitValue.Value)
            {
                return Result.Failure<CreateTenantResponse>(
                    TenantErrors.MaxFreeTenantsReached(plan.Id));
            }
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            SubscriptionPlanId = plan.Id,
            Status = plan.TrialDays > 0 ? SubscriptionStatus.Trialing : SubscriptionStatus.Active,
            BillingPeriod = SubscriptionBillingPeriod.None,
            ExpiresAt = plan.TrialDays > 0 ? DateTime.UtcNow.AddDays(plan.TrialDays) : null,
            CreatedAt = DateTime.UtcNow
        };

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Identifier = command.Identifier,
            CreatedAt = DateTime.UtcNow,
            IsDemoData = command.UseDemoData
        };

        subscription.TenantId = tenant.Id;

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
        context.Subscriptions.Add(subscription);
        context.Roles.Add(ownerRole);
        context.Roles.Add(adminRole);
        context.Roles.Add(memberRole);
        context.Memberships.Add(membership);

        await context.SaveChangesAsync(cancellationToken);

        if (command.UseDemoData)
        {
            await jobQueue.EnqueueAsync(
                new SeedDemoDataJob(tenant.Id),
                cancellationToken: cancellationToken);
        }

        User? user = await context.Users.FirstOrDefaultAsync(
            u => u.Id == command.OwnerId, cancellationToken);

        tenant.Raise(new TenantCreatedDomainEvent(tenant.Id, tenant.Name, user!.Email!));

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
