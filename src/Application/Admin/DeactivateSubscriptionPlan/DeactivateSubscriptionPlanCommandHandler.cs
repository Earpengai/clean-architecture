using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.DeactivateSubscriptionPlan;

internal sealed class DeactivateSubscriptionPlanCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<DeactivateSubscriptionPlanCommand>
{
    public async Task<Result> Handle(
        DeactivateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        SubscriptionPlan? plan = await context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == command.SubscriptionPlanId, cancellationToken);

        if (plan is null)
        {
            return Result.Failure(SubscriptionErrors.PlanNotFound(command.SubscriptionPlanId));
        }

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
