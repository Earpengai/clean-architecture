using Application.Abstractions.Email;
using Domain.Tenants;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Tenants.CreateTenant;

internal sealed class TenantCreatedDomainEventHandler(
    IEmailService emailService,
    IConfiguration configuration)
    : IDomainEventHandler<TenantCreatedDomainEvent>
{
    public async Task Handle(TenantCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string frontendUrl = configuration["FrontendUrl"] ?? string.Empty;
        string linkSection = string.IsNullOrEmpty(frontendUrl)
            ? string.Empty
            : $"<p>Visit your workspace at <a href=\"{frontendUrl}\">{frontendUrl}</a></p>";

        string body = $"""
            <h2>Your workspace is ready!</h2>
            <p>Your tenant <strong>{domainEvent.TenantName}</strong> has been created on the Clean Architecture platform.</p>
            <p>Owner email: <strong>{domainEvent.OwnerEmail}</strong></p>
            <p>You can now sign in and start using your workspace.</p>
            {linkSection}
            <p>Thank you for choosing Clean Architecture!</p>
            """;

        await emailService.SendAsync(
            domainEvent.OwnerEmail,
            $"Workspace created — {domainEvent.TenantName}",
            body,
            cancellationToken);
    }
}
