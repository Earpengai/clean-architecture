using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Jobs;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.ClearDemoData;

internal sealed class ClearDemoDataCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IBackgroundJobQueue jobQueue)
    : ICommandHandler<ClearDemoDataCommand>
{
    public async Task<Result> Handle(ClearDemoDataCommand command, CancellationToken cancellationToken)
    {
        Membership? membership = await context.Memberships
            .Include(m => m.Role)
            .FirstOrDefaultAsync(
                m => m.UserId == userContext.UserId && m.TenantId == command.TenantId,
                cancellationToken);

        if (membership is null || membership.Role.Name != "Owner")
        {
            return Result.Failure(TenantErrors.NotOwner);
        }

        Tenant? tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == command.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantId));
        }

        if (!tenant.IsDemoData)
        {
            return Result.Failure(TenantErrors.NotDemoTenant);
        }

        if (tenant.DemoDataClearedAt is not null)
        {
            return Result.Failure(TenantErrors.DemoDataAlreadyCleared);
        }

        tenant.IsDemoData = false;
        tenant.DemoDataClearedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        await jobQueue.EnqueueAsync(
            new ClearDemoDataJob(command.TenantId),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
