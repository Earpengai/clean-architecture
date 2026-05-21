using Application.Abstractions.Email;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Users.DomainEventHandlers;

internal sealed class SendPasswordResetTokenDomainEventHandler(
    IEmailService emailService,
    IConfiguration configuration) : IDomainEventHandler<PasswordResetRequestedDomainEvent>
{
    public async Task Handle(PasswordResetRequestedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string baseUrl = configuration["Client:BaseUrl"]!;

        string body = $"""
            <h2>Reset your password</h2>
            <p>Please use the following details to reset your password:</p>
            <p><strong>Endpoint:</strong> POST {baseUrl}/users/reset-password</p>
            <p><strong>User ID:</strong> {domainEvent.UserId}</p>
            <p><strong>Token:</strong> {domainEvent.Token}</p>
            """;

        await emailService.SendAsync(domainEvent.Email, "Reset your password", body, cancellationToken);
    }
}
