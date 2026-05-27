using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    ITokenProvider tokenProvider,
    IClientInfoProvider clientInfoProvider) : ICommandHandler<LoginUserCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(command.Email);

        if (user is null)
        {
            return Result.Failure<LoginResponse>(UserErrors.NotFoundByEmail);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<LoginResponse>(UserErrors.AccountLocked);
        }

        bool verified = await userManager.CheckPasswordAsync(user, command.Password);

        if (!verified)
        {
            await userManager.AccessFailedAsync(user);

            return Result.Failure<LoginResponse>(UserErrors.NotFoundByEmail);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (!user.EmailConfirmed)
        {
            return Result.Failure<LoginResponse>(UserErrors.EmailNotVerified);
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            if (command.RememberDeviceToken is not null
                && command.RememberDeviceToken.Length == 128
                && command.RememberDeviceToken.All(c => c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f'))
            {
                string rememberTokenHash = Convert.ToHexString(
                    SHA256.HashData(Convert.FromHexString(command.RememberDeviceToken)));

                bool rememberTokenValid = await context.TwoFactorRememberTokens
                    .AnyAsync(t => t.UserId == user.Id
                        && t.TokenHash == rememberTokenHash
                        && t.ExpiresAt > DateTime.UtcNow, cancellationToken);

                if (rememberTokenValid)
                {
                    return await IssueTokens(user, context, tokenProvider, clientInfoProvider, cancellationToken);
                }
            }

            return LoginResponse.TwoFactorRequired(user.Id);
        }

        return await IssueTokens(user, context, tokenProvider, clientInfoProvider, cancellationToken);
    }

    private static async Task<LoginResponse> IssueTokens(
        User user,
        IApplicationDbContext context,
        ITokenProvider tokenProvider,
        IClientInfoProvider clientInfoProvider,
        CancellationToken cancellationToken)
    {
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

        var refreshTokenId = Guid.NewGuid();

        var refreshToken = new RefreshToken
        {
            Id = refreshTokenId,
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(refreshToken);

        DateTime timestamp = DateTime.UtcNow;

        user.Raise(new UserLoggedInDomainEvent(
            user.Id,
            user.Email!,
            clientInfoProvider.IpAddress,
            clientInfoProvider.UserAgent,
            refreshTokenId,
            timestamp));

        await context.SaveChangesAsync(cancellationToken);

        return LoginResponse.Success(accessToken, plainRefreshToken, user.EmailConfirmed);
    }
}
