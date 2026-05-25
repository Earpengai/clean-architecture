using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.GetSubscriptionPlanById;

internal sealed class GetSubscriptionPlanByIdQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanResponse>
{
    public async Task<Result<SubscriptionPlanResponse>> Handle(
        GetSubscriptionPlanByIdQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<SubscriptionPlanResponse>(UserErrors.Unauthorized());
        }

        SubscriptionPlan? plan = await context.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.SubscriptionPlanId, cancellationToken);

        if (plan is null)
        {
            return Result.Failure<SubscriptionPlanResponse>(
                SubscriptionErrors.PlanNotFound(query.SubscriptionPlanId));
        }

        List<PlanFeatureInfo> features = await context.PlanFeatures
            .AsNoTracking()
            .Where(pf => pf.SubscriptionPlanId == plan.Id)
            .OrderBy(pf => pf.Feature)
            .Select(pf => new PlanFeatureInfo
            {
                Feature = pf.Feature,
                IsEnabled = pf.IsEnabled
            })
            .ToListAsync(cancellationToken);

        List<PlanLimitInfo> limits = await context.PlanLimits
            .AsNoTracking()
            .Where(pl => pl.SubscriptionPlanId == plan.Id)
            .OrderBy(pl => pl.Limit)
            .Select(pl => new PlanLimitInfo
            {
                Limit = pl.Limit,
                Value = pl.Value
            })
            .ToListAsync(cancellationToken);

        return new SubscriptionPlanResponse
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            PriceMonthly = plan.PriceMonthly,
            PriceYearly = plan.PriceYearly,
            TrialDays = plan.TrialDays,
            SortOrder = plan.SortOrder,
            IsActive = plan.IsActive,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt,
            Features = features,
            Limits = limits
        };
    }
}
