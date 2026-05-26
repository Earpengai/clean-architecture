namespace Application.Users.Register;

public sealed record RegisterResponse(Guid UserId, bool VerificationEmailSent);
