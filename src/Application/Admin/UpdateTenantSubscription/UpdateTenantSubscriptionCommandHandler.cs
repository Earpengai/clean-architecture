using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.UpdateTenantSubscription;

internal sealed class UpdateTenantSubscriptionCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateTenantSubscriptionCommand>
{
    public async Task<Result> Handle(UpdateTenantSubscriptionCommand command, CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        Tenant? tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == command.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantId));
        }

        Subscription? subscription = await context.Subscriptions
            .FirstOrDefaultAsync(s => s.TenantId == command.TenantId, cancellationToken);

        if (subscription is null)
        {
            subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = command.TenantId,
                CreatedAt = DateTime.UtcNow
            };
            context.Subscriptions.Add(subscription);
        }

        subscription.SubscriptionPlanId = command.SubscriptionPlanId;
        subscription.Status = command.SubscriptionStatus;
        subscription.MaxUsersOverride = command.MaxUsersOverride;
        subscription.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
