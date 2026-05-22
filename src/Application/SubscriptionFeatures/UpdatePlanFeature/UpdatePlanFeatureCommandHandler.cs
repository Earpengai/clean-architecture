using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.SubscriptionFeatures.UpdatePlanFeature;

internal sealed class UpdatePlanFeatureCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdatePlanFeatureCommand>
{
    public async Task<Result> Handle(UpdatePlanFeatureCommand command, CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure(Domain.Users.UserErrors.Unauthorized());
        }

        PlanFeature? planFeature = await context.PlanFeatures
            .FirstOrDefaultAsync(pf => pf.Plan == command.Plan && pf.Feature == command.Feature, cancellationToken);

        if (planFeature is not null)
        {
            planFeature.IsEnabled = command.IsEnabled;
        }
        else
        {
            planFeature = new PlanFeature
            {
                Plan = command.Plan,
                Feature = command.Feature,
                IsEnabled = command.IsEnabled
            };

            context.PlanFeatures.Add(planFeature);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
