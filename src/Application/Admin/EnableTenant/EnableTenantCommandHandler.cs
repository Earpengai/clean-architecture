using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.EnableTenant;

internal sealed class EnableTenantCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<EnableTenantCommand>
{
    public async Task<Result> Handle(EnableTenantCommand command, CancellationToken cancellationToken)
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

        tenant.IsActive = true;
        tenant.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
