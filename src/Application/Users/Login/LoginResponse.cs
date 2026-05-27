namespace Application.Users.Login;

public sealed record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    Guid? UserId,
    bool EmailConfirmed,
    string? RememberDeviceToken)
{
    public static LoginResponse Success(string accessToken, string refreshToken, bool emailConfirmed) =>
        new(accessToken, refreshToken, false, null, emailConfirmed, null);

    public static LoginResponse TwoFactorRequired(Guid userId) =>
        new(null, null, true, userId, false, null);

    public static LoginResponse Success(string accessToken, string refreshToken, bool emailConfirmed, string rememberDeviceToken) =>
        new(accessToken, refreshToken, false, null, emailConfirmed, rememberDeviceToken);
}
