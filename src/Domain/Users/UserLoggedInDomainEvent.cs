using SharedKernel;

namespace Domain.Users;

public sealed record UserLoggedInDomainEvent(
    Guid UserId,
    string Email,
    string? IpAddress,
    string? UserAgent,
    Guid RefreshTokenId,
    DateTime Timestamp) : IDomainEvent;
