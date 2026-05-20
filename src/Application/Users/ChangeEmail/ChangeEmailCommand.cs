using Application.Abstractions.Messaging;

namespace Application.Users.ChangeEmail;

public sealed record ChangeEmailCommand(string NewEmail) : ICommand;
