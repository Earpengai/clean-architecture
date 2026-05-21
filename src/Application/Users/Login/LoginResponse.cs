namespace Application.Users.Login;

public sealed record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    string? TwoFactorToken,
    Guid? UserId)
{
    public static LoginResponse Success(string accessToken, string refreshToken) =>
        new(accessToken, refreshToken, false, null, null);

    public static LoginResponse TwoFactorRequired(string twoFactorToken, Guid userId) =>
        new(null, null, true, twoFactorToken, userId);
}
