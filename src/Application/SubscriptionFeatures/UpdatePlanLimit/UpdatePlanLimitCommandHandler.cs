using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.SubscriptionFeatures.UpdatePlanLimit;

internal sealed class UpdatePlanLimitCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdatePlanLimitCommand>
{
    public async Task<Result> Handle(UpdatePlanLimitCommand command, CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure(Domain.Users.UserErrors.Unauthorized());
        }

        SubscriptionPlan? plan = await context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == command.SubscriptionPlanId, cancellationToken);

        if (plan is null)
        {
            return Result.Failure(SubscriptionErrors.PlanNotFound(command.SubscriptionPlanId));
        }

        PlanLimit? planLimit = await context.PlanLimits
            .FirstOrDefaultAsync(pl => pl.SubscriptionPlanId == command.SubscriptionPlanId && pl.Limit == command.Limit, cancellationToken);

        if (planLimit is not null)
        {
            planLimit.Value = command.Value;
        }
        else
        {
            planLimit = new PlanLimit
            {
                SubscriptionPlanId = command.SubscriptionPlanId,
                Limit = command.Limit,
                Value = command.Value
            };

            context.PlanLimits.Add(planLimit);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
