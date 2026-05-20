using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.DisableTenant;

internal sealed class DisableTenantCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<DisableTenantCommand>
{
    public async Task<Result> Handle(DisableTenantCommand command, CancellationToken cancellationToken)
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

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
