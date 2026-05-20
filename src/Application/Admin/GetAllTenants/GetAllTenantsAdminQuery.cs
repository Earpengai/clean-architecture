using Application.Abstractions.Messaging;

namespace Application.Admin.GetAllTenants;

public sealed record GetAllTenantsAdminQuery : IQuery<List<TenantAdminResponse>>;
