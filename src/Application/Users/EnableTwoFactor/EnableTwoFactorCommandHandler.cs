using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.EnableTwoFactor;

internal sealed class EnableTwoFactorCommandHandler(
    IUserContext userContext,
    UserManager<User> userManager) : ICommandHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
{
    public async Task<Result<EnableTwoFactorResponse>> Handle(
        EnableTwoFactorCommand command,
        CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());

        if (user is null)
        {
            return Result.Failure<EnableTwoFactorResponse>(UserErrors.NotFound(userContext.UserId));
        }

        string key = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        string authenticatorUri = GenerateQrCodeUri(user.Email!, key!);

        return new EnableTwoFactorResponse(key!, new Uri(authenticatorUri));
    }

    private static string GenerateQrCodeUri(string email, string key)
    {
        string encodedEmail = Uri.EscapeDataString(email);
        string encodedKey = Uri.EscapeDataString(key);

        return $"otpauth://totp/CleanArchitecture:{encodedEmail}?secret={encodedKey}&issuer=CleanArchitecture";
    }
}
