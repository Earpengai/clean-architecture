using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.RemoveUserFromTenant;

internal sealed class RemoveUserFromTenantCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<RemoveUserFromTenantCommand>
{
    public async Task<Result> Handle(RemoveUserFromTenantCommand command, CancellationToken cancellationToken)
    {
        Membership? membership = await context.Memberships
            .FirstOrDefaultAsync(m => m.UserId == command.UserId
                && m.TenantId == userContext.TenantId!.Value, cancellationToken);

        if (membership is null)
        {
            return Result.Failure(UserErrors.NotMemberOfTenant);
        }

        context.Memberships.Remove(membership);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
