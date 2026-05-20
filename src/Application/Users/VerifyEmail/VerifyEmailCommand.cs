using Application.Abstractions.Messaging;

namespace Application.Users.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : ICommand;
