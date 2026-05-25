using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.UpdateSubscriptionStatus;

internal sealed class UpdateSubscriptionStatusCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateSubscriptionStatusCommand>
{
    public async Task<Result> Handle(
        UpdateSubscriptionStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        Subscription? subscription = await context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == command.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure(SubscriptionErrors.NotFound(command.SubscriptionId));
        }

        subscription.Status = command.NewStatus;
        subscription.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
