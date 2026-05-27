using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Login;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.LoginRecovery;

internal sealed class LoginRecoveryCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    ITokenProvider tokenProvider,
    IClientInfoProvider clientInfoProvider) : ICommandHandler<LoginRecoveryCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginRecoveryCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure<LoginResponse>(UserErrors.NotFound(command.UserId));
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<LoginResponse>(UserErrors.AccountLocked);
        }

        IdentityResult recoveryResult = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, command.RecoveryCode);

        if (!recoveryResult.Succeeded)
        {
            return Result.Failure<LoginResponse>(UserErrors.InvalidRecoveryCode);
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
