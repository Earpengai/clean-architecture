using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Users.Login;

public sealed record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    string? TwoFactorToken)
{
    public static LoginResponse Success(string accessToken, string refreshToken) =>
        new(accessToken, refreshToken, false, null);

    public static LoginResponse TwoFactorRequired(string twoFactorToken) =>
        new(null, null, true, twoFactorToken);
}
