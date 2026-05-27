namespace Application.Users.ConfirmTwoFactor;

public sealed record ConfirmTwoFactorResponse(List<string> RecoveryCodes);
