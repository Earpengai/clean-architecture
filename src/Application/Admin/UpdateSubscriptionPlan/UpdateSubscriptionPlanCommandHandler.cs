using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.UpdateSubscriptionPlan;

internal sealed class UpdateSubscriptionPlanCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateSubscriptionPlanCommand>
{
    public async Task<Result> Handle(
        UpdateSubscriptionPlanCommand command,
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

        bool nameExists = await context.SubscriptionPlans
            .AnyAsync(p => p.Name == command.Name && p.Id != command.SubscriptionPlanId, cancellationToken);

        if (nameExists)
        {
            return Result.Failure(SubscriptionErrors.PlanNameAlreadyExists(command.Name));
        }

        plan.Name = command.Name;
        plan.Description = command.Description;
        plan.PriceMonthly = command.PriceMonthly;
        plan.PriceYearly = command.PriceYearly;
        plan.TrialDays = command.TrialDays;
        plan.SortOrder = command.SortOrder;
        plan.IsActive = command.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
