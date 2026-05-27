using Application.Abstractions.Messaging;

namespace Application.Users.GetUserSessions;

public sealed record GetUserSessionsQuery : IQuery<List<UserSessionResponse>>;
