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

        List<PlanLimitResponse> limits = await context.PlanLimits
            .AsNoTracking()
            .OrderBy(pl => pl.Plan)
            .ThenBy(pl => pl.Limit)
            .Select(pl => new PlanLimitResponse
            {
                Plan = pl.Plan.ToString(),
                Limit = pl.Limit,
                Value = pl.Value
            })
            .ToListAsync(cancellationToken);

        return limits;
    }
}
