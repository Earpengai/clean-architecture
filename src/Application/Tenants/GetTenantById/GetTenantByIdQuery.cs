using Application.Abstractions.Messaging;

namespace Application.Tenants.GetTenantById;

public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantResponse>;
