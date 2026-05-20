using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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

        tenant.SubscriptionPlan = command.SubscriptionPlan;
        tenant.SubscriptionStatus = command.SubscriptionStatus;
        tenant.SeatCount = command.SeatCount;
        tenant.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
