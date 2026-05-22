namespace Application.Tenants.AcceptInvitation;

public sealed record AcceptInvitationResponse(Guid UserId, string AccessToken, string RefreshToken);
