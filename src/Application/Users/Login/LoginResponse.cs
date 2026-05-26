namespace Application.Users.Login;

public sealed record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    string? TwoFactorToken,
    Guid? UserId,
    bool EmailConfirmed)
{
    public static LoginResponse Success(string accessToken, string refreshToken, bool emailConfirmed) =>
        new(accessToken, refreshToken, false, null, null, emailConfirmed);

    public static LoginResponse TwoFactorRequired(string twoFactorToken, Guid userId) =>
        new(null, null, true, twoFactorToken, userId, false);
}
