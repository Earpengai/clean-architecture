using Application.Abstractions.Data;
using Application.Abstractions.Jobs;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.DomainEventHandlers;

internal sealed class DetectSuspiciousLoginDomainEventHandler(
    IApplicationDbContext context,
    IBackgroundJobQueue backgroundJobQueue) : IDomainEventHandler<UserLoggedInDomainEvent>
{
    public async Task Handle(UserLoggedInDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        bool hasExistingSessions = await context.UserSessions
            .AnyAsync(s => s.UserId == domainEvent.UserId
                && s.IsActive
                && s.RefreshTokenId != domainEvent.RefreshTokenId, cancellationToken);

        string browser = UserAgentParser.ParseBrowser(domainEvent.UserAgent ?? string.Empty);
        string os = UserAgentParser.ParseOperatingSystem(domainEvent.UserAgent ?? string.Empty);
        string ipAddress = domainEvent.IpAddress ?? "unknown";

        if (!hasExistingSessions)
        {
            var job = new SuspiciousLoginEmailJob(
                domainEvent.UserId,
                domainEvent.Email,
                ipAddress,
                browser,
                os,
                domainEvent.Timestamp,
                IsFirstSession: true);

            await backgroundJobQueue.EnqueueAsync(job, cancellationToken: cancellationToken);

            return;
        }

        bool ipSeenBefore = await context.UserSessions
            .AnyAsync(s => s.UserId == domainEvent.UserId
                && s.IsActive
                && s.IpAddress == ipAddress
                && s.RefreshTokenId != domainEvent.RefreshTokenId, cancellationToken);

        bool deviceSeenBefore = await context.UserSessions
            .AnyAsync(s => s.UserId == domainEvent.UserId
                && s.IsActive
                && s.Browser == browser
                && s.OperatingSystem == os
                && s.RefreshTokenId != domainEvent.RefreshTokenId, cancellationToken);

        if (!ipSeenBefore || !deviceSeenBefore)
        {
            var job = new SuspiciousLoginEmailJob(
                domainEvent.UserId,
                domainEvent.Email,
                ipAddress,
                browser,
                os,
                domainEvent.Timestamp,
                IsFirstSession: false);

            await backgroundJobQueue.EnqueueAsync(job, cancellationToken: cancellationToken);
        }
    }
}
