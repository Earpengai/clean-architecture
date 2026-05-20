using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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

        List<TenantAdminResponse> tenants = await context.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantAdminResponse
            {
                Id = t.Id,
                Name = t.Name,
                Identifier = t.Identifier,
                SubscriptionPlan = t.SubscriptionPlan,
                SubscriptionStatus = t.SubscriptionStatus,
                SeatCount = t.SeatCount,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return tenants;
    }
}
