using SharedKernel;

namespace Application.Tenants.ClearDemoData;

public sealed record ClearDemoDataJob(Guid TenantId) : IBackgroundJob;
