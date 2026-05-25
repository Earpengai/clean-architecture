using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.GetAllSubscriptions;

internal sealed class GetAllSubscriptionsQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetAllSubscriptionsQuery, List<SubscriptionResponse>>
{
    public async Task<Result<List<SubscriptionResponse>>> Handle(
        GetAllSubscriptionsQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<List<SubscriptionResponse>>(UserErrors.Unauthorized());
        }

        List<SubscriptionResponse> subscriptions = await context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Tenant)
            .Include(s => s.SubscriptionPlan)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubscriptionResponse
            {
                Id = s.Id,
                TenantId = s.TenantId,
                TenantName = s.Tenant!.Name,
                PlanName = s.SubscriptionPlan!.Name,
                Status = s.Status.ToString(),
                BillingPeriod = s.BillingPeriod.ToString(),
                ExpiresAt = s.ExpiresAt,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return subscriptions;
    }
}
