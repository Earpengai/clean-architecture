using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.GetTenantById;

internal sealed class GetTenantByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    public async Task<Result<TenantResponse>> Handle(
        GetTenantByIdQuery query,
        CancellationToken cancellationToken)
    {
        Domain.Tenants.Tenant? tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<TenantResponse>(
                Domain.Tenants.TenantErrors.NotFound(query.TenantId));
        }

        return new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Identifier,
            tenant.SubscriptionPlan,
            tenant.SubscriptionStatus,
            tenant.SeatCount);
    }
}
