using SharedKernel;

namespace Domain.Tenants;

public sealed record InvitationCreatedDomainEvent(Guid InvitationId, string Email, string Token, string TenantName) : IDomainEvent;
