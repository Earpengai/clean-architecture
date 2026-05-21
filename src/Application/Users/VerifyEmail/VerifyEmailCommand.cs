using Application.Abstractions.Messaging;

namespace Application.Users.VerifyEmail;

public sealed record VerifyEmailCommand(Guid UserId, string Token) : ICommand;
