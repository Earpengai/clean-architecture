using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.GetAllTenants;

internal sealed class GetAllTenantsAdminQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetAllTenantsAdminQuery, List<TenantAdminResponse>>
{
    public async Task<Result<List<TenantAdminResponse>>> Handle(
        GetAllTenantsAdminQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<List<TenantAdminResponse>>(
                Domain.Users.UserErrors.Unauthorized());
        }

#pragma warning disable IDE0031
        List<TenantAdminResponse> tenants = await context.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantAdminResponse
            {
                Id = t.Id,
                Name = t.Name,
                Identifier = t.Identifier,
                SubscriptionPlanName = t.Subscription != null ? t.Subscription.SubscriptionPlan!.Name : null,
                SubscriptionStatus = t.Subscription != null ? t.Subscription.Status : (SubscriptionStatus?)null,
                SeatCount = t.SeatCount,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
#pragma warning restore IDE0031

        return tenants;
    }
}
