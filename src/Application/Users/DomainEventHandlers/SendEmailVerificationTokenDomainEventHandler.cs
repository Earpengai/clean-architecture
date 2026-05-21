using Application.Abstractions.Email;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Users.DomainEventHandlers;

internal sealed class SendEmailVerificationTokenDomainEventHandler(
    IEmailService emailService,
    IConfiguration configuration) : IDomainEventHandler<EmailVerificationRequestedDomainEvent>
{
    public async Task Handle(EmailVerificationRequestedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string baseUrl = configuration["Client:BaseUrl"]!;

        string body = $"""
            <h2>Verify your email address</h2>
            <p>Please use the following details to verify your email address:</p>
            <p><strong>Endpoint:</strong> POST {baseUrl}/users/verify-email</p>
            <p><strong>User ID:</strong> {domainEvent.UserId}</p>
            <p><strong>Token:</strong> {domainEvent.Token}</p>
            """;

        await emailService.SendAsync(domainEvent.Email, "Verify your email address", body, cancellationToken);
    }
}
