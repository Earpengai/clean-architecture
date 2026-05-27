using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.GetTenantById;

internal sealed class GetTenantByIdQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    public async Task<Result<TenantResponse>> Handle(
        GetTenantByIdQuery query,
        CancellationToken cancellationToken)
    {
        Domain.Tenants.Tenant? tenant = await context.Tenants
            .AsNoTracking()
            .Include(t => t.Subscription)
            .ThenInclude(s => s!.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == query.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<TenantResponse>(
                Domain.Tenants.TenantErrors.NotFound(query.TenantId));
        }

        string? role = await context.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == userContext.UserId)
            .Select(m => m.Role.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Identifier,
            tenant.Subscription?.SubscriptionPlan?.Name,
            tenant.Subscription?.Status,
            tenant.Subscription?.MaxUsersOverride,
            role,
            tenant.Subscription?.BillingPeriod.ToString(),
            tenant.Subscription?.ExpiresAt,
            tenant.IsActive,
            tenant.IsDemoData,
            tenant.DemoDataClearedAt);
    }
}
