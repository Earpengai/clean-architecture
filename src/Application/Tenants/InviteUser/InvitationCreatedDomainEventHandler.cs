using Application.Abstractions.Email;
using Domain.Tenants;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Tenants.InviteUser;

internal sealed class InvitationCreatedDomainEventHandler(IEmailService emailService, IConfiguration configuration)
    : IDomainEventHandler<InvitationCreatedDomainEvent>
{
    public async Task Handle(InvitationCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string frontendUrl = configuration["FrontendUrl"] ?? string.Empty;
        string acceptLink = string.IsNullOrEmpty(frontendUrl)
            ? $"/invitations/{domainEvent.Token}/accept"
            : $"{frontendUrl}/invitations/{domainEvent.Token}/accept";

        string body = $"""
            <h2>You've been invited to join {domainEvent.TenantName}!</h2>
            <p>You have been invited to join the tenant <strong>{domainEvent.TenantName}</strong> on the Clean Architecture platform.</p>
            <p>If you already have an account, you can accept this invitation directly from the platform by visiting your invitations page.</p>
            <p>Otherwise, use the invitation link below to create your account and join:</p>
            <p><a href="{acceptLink}">Accept Invitation</a></p>
            <p>This invitation expires in 7 days.</p>
            """;

        await emailService.SendAsync(domainEvent.Email, $"You've been invited to join {domainEvent.TenantName}", body, cancellationToken);
    }
}
