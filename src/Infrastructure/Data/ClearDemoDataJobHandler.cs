using Application.Abstractions.Data;
using Application.Tenants.ClearDemoData;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Data;

internal sealed class ClearDemoDataJobHandler(
    IApplicationDbContext context,
    ILogger<ClearDemoDataJobHandler> logger)
    : IBackgroundJobHandler<ClearDemoDataJob>
{
    public async Task Handle(ClearDemoDataJob job, CancellationToken cancellationToken)
    {
        logger.LogDebug("Clearing demo data for tenant {TenantId}", job.TenantId);

        int deletedCount = await context.TodoItems
            .Where(t => t.TenantId == job.TenantId)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogDebug(
            "Cleared {Count} demo todo items for tenant {TenantId}",
            deletedCount,
            job.TenantId);
    }
}
