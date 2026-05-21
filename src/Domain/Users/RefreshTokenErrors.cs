using SharedKernel;

namespace Domain.Users;

public static class RefreshTokenErrors
{
    public static readonly Error InvalidOrExpired = Error.Problem(
        "RefreshToken.InvalidOrExpired",
        "The refresh token is invalid, expired, or has been revoked.");

    public static readonly Error ReuseDetected = Error.Problem(
        "RefreshToken.ReuseDetected",
        "This refresh token has already been used. All tokens have been revoked for security.");
}
