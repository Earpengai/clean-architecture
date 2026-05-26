using SharedKernel;

namespace Domain.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId, string Email, string Token) : IDomainEvent;
