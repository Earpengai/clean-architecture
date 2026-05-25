using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.SubscriptionFeatures.GetPlanFeatures;

internal sealed class GetPlanFeaturesQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetPlanFeaturesQuery, List<PlanFeatureResponse>>
{
    public async Task<Result<List<PlanFeatureResponse>>> Handle(
        GetPlanFeaturesQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<List<PlanFeatureResponse>>(
                Domain.Users.UserErrors.Unauthorized());
        }

#pragma warning disable IDE0031
        List<PlanFeatureResponse> features = await context.PlanFeatures
            .AsNoTracking()
            .OrderBy(pf => pf.SubscriptionPlan != null ? pf.SubscriptionPlan.Name : string.Empty)
            .ThenBy(pf => pf.Feature)
            .Select(pf => new PlanFeatureResponse
            {
                SubscriptionPlanId = pf.SubscriptionPlanId,
                Plan = pf.SubscriptionPlan != null ? pf.SubscriptionPlan.Name : string.Empty,
                Feature = pf.Feature,
                IsEnabled = pf.IsEnabled
            })
            .ToListAsync(cancellationToken);
#pragma warning restore IDE0031

        return features;
    }
}
