using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.GetAllSubscriptionPlans;

internal sealed class GetAllSubscriptionPlansQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetAllSubscriptionPlansQuery, List<SubscriptionPlanResponse>>
{
    public async Task<Result<List<SubscriptionPlanResponse>>> Handle(
        GetAllSubscriptionPlansQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<List<SubscriptionPlanResponse>>(UserErrors.Unauthorized());
        }

        List<SubscriptionPlanResponse> plans = await context.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.SortOrder)
            .Select(p => new SubscriptionPlanResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                PriceMonthly = p.PriceMonthly,
                PriceYearly = p.PriceYearly,
                TrialDays = p.TrialDays,
                SortOrder = p.SortOrder,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                SubscriptionCount = context.Subscriptions.Count(s => s.SubscriptionPlanId == p.Id)
            })
            .ToListAsync(cancellationToken);

        return plans;
    }
}
