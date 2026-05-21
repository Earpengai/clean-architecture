namespace Application.Users.EnableTwoFactor;

public sealed record EnableTwoFactorResponse(string SharedKey, Uri AuthenticatorUri);
