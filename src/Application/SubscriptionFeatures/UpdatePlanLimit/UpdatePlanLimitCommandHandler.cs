using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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

        PlanLimit? planLimit = await context.PlanLimits
            .FirstOrDefaultAsync(pl => pl.Plan == command.Plan && pl.Limit == command.Limit, cancellationToken);

        if (planLimit is not null)
        {
            planLimit.Value = command.Value;
        }
        else
        {
            planLimit = new PlanLimit
            {
                Plan = command.Plan,
                Limit = command.Limit,
                Value = command.Value
            };

            context.PlanLimits.Add(planLimit);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
