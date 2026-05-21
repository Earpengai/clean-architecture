using Application.Abstractions.Email;
using Domain.Users;
using SharedKernel;

namespace Application.Users.DomainEventHandlers;

internal sealed class UserRegisteredDomainEventHandler(IEmailService emailService)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string body = $"""
            <h2>Welcome to Clean Architecture!</h2>
            <p>Your account has been created successfully with the email <strong>{domainEvent.Email}</strong>.</p>
            <p>Please verify your email address to get started with the platform.</p>
            """;

        await emailService.SendAsync(domainEvent.Email, "Welcome to Clean Architecture", body, cancellationToken);
    }
}
