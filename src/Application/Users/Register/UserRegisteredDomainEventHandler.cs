using Application.Abstractions.Email;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Users.DomainEventHandlers;

internal sealed class UserRegisteredDomainEventHandler(
    IEmailService emailService,
    IConfiguration configuration)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        bool sendVerification = configuration.GetValue<bool>("EmailVerification:SendVerificationEmail");
        string frontendUrl = configuration["FrontendUrl"] ?? string.Empty;

        string verificationSection = string.Empty;

        if (sendVerification && !string.IsNullOrEmpty(frontendUrl))
        {
            string verifyLink = $"{frontendUrl}/auth/verify-email?userId={domainEvent.UserId}&token={Uri.EscapeDataString(domainEvent.Token)}";

            verificationSection = $"""
                <p>To verify your email address, click the link below:</p>
                <p><a href="{verifyLink}">Verify Email Address</a></p>
                <p>Or copy and paste this link into your browser:</p>
                <p>{verifyLink}</p>
                """;
        }

        string body = $"""
            <h2>Welcome to Clean Architecture!</h2>
            <p>Your account has been created successfully with the email <strong>{domainEvent.Email}</strong>.</p>
            {verificationSection}
            <p>Thank you for joining us!</p>
            """;

        await emailService.SendAsync(domainEvent.Email, "Welcome to Clean Architecture", body, cancellationToken);
    }
}
