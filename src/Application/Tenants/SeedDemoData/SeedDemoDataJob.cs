using SharedKernel;

namespace Application.Tenants.SeedDemoData;

public sealed record SeedDemoDataJob(Guid TenantId) : IBackgroundJob;
