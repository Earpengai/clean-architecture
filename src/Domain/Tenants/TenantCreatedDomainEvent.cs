using SharedKernel;

namespace Domain.Tenants;

public sealed record TenantCreatedDomainEvent(Guid TenantId, string TenantName, string OwnerEmail) : IDomainEvent;
