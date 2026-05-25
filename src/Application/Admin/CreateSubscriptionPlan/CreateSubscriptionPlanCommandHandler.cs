using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.CreateSubscriptionPlan;

internal sealed class CreateSubscriptionPlanCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<CreateSubscriptionPlanCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized());
        }

        bool nameExists = await context.SubscriptionPlans
            .AnyAsync(p => p.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(SubscriptionErrors.PlanNameAlreadyExists(command.Name));
        }

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            PriceMonthly = command.PriceMonthly,
            PriceYearly = command.PriceYearly,
            TrialDays = command.TrialDays,
            SortOrder = command.SortOrder,
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        context.SubscriptionPlans.Add(plan);

        await context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
