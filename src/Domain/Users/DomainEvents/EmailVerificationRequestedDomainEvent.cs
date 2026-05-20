using SharedKernel;

namespace Domain.Users;

public sealed record EmailVerificationRequestedDomainEvent(Guid UserId, string Email) : IDomainEvent;
