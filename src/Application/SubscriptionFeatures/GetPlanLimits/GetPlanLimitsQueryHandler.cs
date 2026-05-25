using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.SubscriptionFeatures.GetPlanLimits;

internal sealed class GetPlanLimitsQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetPlanLimitsQuery, List<PlanLimitResponse>>
{
    public async Task<Result<List<PlanLimitResponse>>> Handle(
        GetPlanLimitsQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<List<PlanLimitResponse>>(
                Domain.Users.UserErrors.Unauthorized());
        }

#pragma warning disable IDE0031
        List<PlanLimitResponse> limits = await context.PlanLimits
            .AsNoTracking()
            .OrderBy(pl => pl.SubscriptionPlan != null ? pl.SubscriptionPlan.Name : string.Empty)
            .ThenBy(pl => pl.Limit)
            .Select(pl => new PlanLimitResponse
            {
                Plan = pl.SubscriptionPlan != null ? pl.SubscriptionPlan.Name : string.Empty,
                Limit = pl.Limit,
                Value = pl.Value
            })
            .ToListAsync(cancellationToken);
#pragma warning restore IDE0031

        return limits;
    }
}
