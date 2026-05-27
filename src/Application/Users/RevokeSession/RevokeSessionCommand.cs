using Application.Abstractions.Messaging;

namespace Application.Users.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand;
