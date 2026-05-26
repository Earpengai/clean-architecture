using Application.Abstractions.Messaging;

namespace Application.Users.ResendVerification;

public sealed record ResendVerificationCommand(Guid UserId) : ICommand;
