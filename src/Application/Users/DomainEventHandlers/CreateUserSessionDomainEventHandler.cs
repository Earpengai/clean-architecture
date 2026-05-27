using Application.Abstractions.Data;
using Domain.Users;
using SharedKernel;

namespace Application.Users.DomainEventHandlers;

internal sealed class CreateUserSessionDomainEventHandler(
    IApplicationDbContext context) : IDomainEventHandler<UserLoggedInDomainEvent>
{
    public async Task Handle(UserLoggedInDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string userAgent = domainEvent.UserAgent ?? string.Empty;
        string browser = UserAgentParser.ParseBrowser(userAgent);
        string operatingSystem = UserAgentParser.ParseOperatingSystem(userAgent);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = domainEvent.UserId,
            RefreshTokenId = domainEvent.RefreshTokenId,
            IpAddress = domainEvent.IpAddress ?? "unknown",
            UserAgent = userAgent,
            Browser = browser,
            OperatingSystem = operatingSystem,
            IsActive = true,
            CreatedAt = domainEvent.Timestamp,
            LastActivityAt = domainEvent.Timestamp
        };

        context.UserSessions.Add(session);

        await context.SaveChangesAsync(cancellationToken);
    }
}
