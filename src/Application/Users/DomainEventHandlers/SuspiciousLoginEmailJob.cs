using SharedKernel;

namespace Application.Users.DomainEventHandlers;

public sealed record SuspiciousLoginEmailJob(
    Guid UserId,
    string Email,
    string IpAddress,
    string Browser,
    string Os,
    DateTime Timestamp,
    bool IsFirstSession) : IBackgroundJob;
