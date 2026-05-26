using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Login;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.LoginTwoFactor;

internal sealed class LoginTwoFactorCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    ITokenProvider tokenProvider) : ICommandHandler<LoginTwoFactorCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginTwoFactorCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure<LoginResponse>(UserErrors.NotFound(command.UserId));
        }

        bool tokenValid = await userManager.VerifyTwoFactorTokenAsync(
            user, TokenOptions.DefaultEmailProvider, command.TwoFactorToken);

        if (!tokenValid)
        {
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);
        }

        bool codeValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, command.Code);

        if (!codeValid)
        {
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);
        }

        if (!user.EmailConfirmed)
        {
            return Result.Failure<LoginResponse>(UserErrors.EmailNotVerified);
        }

        List<string> tenantIdentifiers = await context.Memberships
            .Where(m => m.UserId == user.Id)
            .Join(context.Tenants,
                m => m.TenantId,
                t => t.Id,
                (m, t) => t.Identifier)
            .ToListAsync(cancellationToken);

        string accessToken = tokenProvider.Create(user, tenantIdentifiers, user.IsSystemAdministrator);

        string plainRefreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        string refreshTokenHash = Convert.ToHexString(SHA256.HashData(Convert.FromHexString(plainRefreshToken)));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync(cancellationToken);

        return LoginResponse.Success(accessToken, plainRefreshToken, user.EmailConfirmed);
    }
}
