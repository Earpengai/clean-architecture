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
        List<TenantResponse> tenants = await context.Memberships
            .Where(m => m.UserId == query.UserId)
            .Join(context.Tenants,
                m => m.TenantId,
                t => t.Id,
                (m, t) => new TenantResponse(
                    t.Id,
                    t.Name,
                    t.Identifier,
                    t.SubscriptionPlan,
                    t.SubscriptionStatus,
                    t.SeatCount,
                    m.Role))
            .ToListAsync(cancellationToken);

        return tenants;
    }
}
