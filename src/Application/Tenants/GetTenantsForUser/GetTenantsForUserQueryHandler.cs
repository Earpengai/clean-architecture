using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.GetTenantsForUser;

internal sealed class GetTenantsForUserQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTenantsForUserQuery, List<TenantResponse>>
{
    public async Task<Result<List<TenantResponse>>> Handle(
        GetTenantsForUserQuery query,
        CancellationToken cancellationToken)
    {
#pragma warning disable IDE0031
        List<TenantResponse> tenants = await context.Memberships
            .Where(m => m.UserId == query.UserId)
            .Join(context.Tenants,
                m => m.TenantId,
                t => t.Id,
                (m, t) => new { Membership = m, Tenant = t })
            .Join(context.Roles,
                x => x.Membership.RoleId,
                r => r.Id,
                (x, r) => new TenantResponse(
                    x.Tenant.Id,
                    x.Tenant.Name,
                    x.Tenant.Identifier,
                    x.Tenant.Subscription != null ? x.Tenant.Subscription.SubscriptionPlan!.Name : null,
                    x.Tenant.Subscription != null ? x.Tenant.Subscription.Status : (SubscriptionStatus?)null,
                    x.Tenant.Subscription != null ? x.Tenant.Subscription.MaxUsersOverride : null,
                    r.Name!,
                    x.Tenant.IsActive,
                    x.Tenant.Subscription != null ? x.Tenant.Subscription.ExpiresAt : null,
                    x.Tenant.IsDemoData,
                    x.Tenant.DemoDataClearedAt))
            .ToListAsync(cancellationToken);
#pragma warning restore IDE0031

        return tenants;
    }
}
