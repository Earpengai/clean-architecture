namespace Application.Users.GetUserSessions;

public sealed record UserSessionResponse(
    Guid Id,
    string Browser,
    string OperatingSystem,
    string IpAddress,
    DateTime CreatedAt,
    DateTime LastActivityAt);
