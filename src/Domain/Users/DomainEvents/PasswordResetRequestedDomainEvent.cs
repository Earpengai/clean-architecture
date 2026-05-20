using SharedKernel;

namespace Domain.Users;

public sealed record PasswordResetRequestedDomainEvent(Guid UserId, string Email) : IDomainEvent;
