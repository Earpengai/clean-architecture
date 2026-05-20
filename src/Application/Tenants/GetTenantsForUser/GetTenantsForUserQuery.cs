using Application.Abstractions.Messaging;

namespace Application.Tenants.GetTenantsForUser;

public sealed record GetTenantsForUserQuery(Guid UserId) : IQuery<List<TenantResponse>>;
