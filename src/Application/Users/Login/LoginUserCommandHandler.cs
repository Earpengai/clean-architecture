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
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, LoginResponse>
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

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            string twoFactorToken = await userManager.GenerateTwoFactorTokenAsync(
                user, TokenOptions.DefaultEmailProvider);

            return LoginResponse.TwoFactorRequired(twoFactorToken, user.Id);
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

        return LoginResponse.Success(accessToken, plainRefreshToken);
    }
}
