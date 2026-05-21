using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Refresh;

internal sealed class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider) : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        string hashedToken = Convert.ToHexString(SHA256.HashData(Convert.FromHexString(command.RefreshToken)));

        RefreshToken? storedToken = await context.RefreshTokens
            .Include(r => r.User)
            .SingleOrDefaultAsync(r => r.TokenHash == hashedToken, cancellationToken);

        if (storedToken is null || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Failure<RefreshTokenResponse>(RefreshTokenErrors.InvalidOrExpired);
        }

        if (storedToken.RevokedAt is not null)
        {
            await RevokeAllUserTokens(storedToken.UserId, storedToken.TokenHash, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<RefreshTokenResponse>(RefreshTokenErrors.ReuseDetected);
        }

        string newPlainToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        string newTokenHash = Convert.ToHexString(SHA256.HashData(Convert.FromHexString(newPlainToken)));

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = newTokenHash;

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = storedToken.UserId,
            TokenHash = newTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(newRefreshToken);

        List<string> tenantIdentifiers = await context.Memberships
            .Where(m => m.UserId == storedToken.UserId)
            .Join(context.Tenants,
                m => m.TenantId,
                t => t.Id,
                (m, t) => t.Identifier)
            .ToListAsync(cancellationToken);

        string accessToken = tokenProvider.Create(storedToken.User, tenantIdentifiers, storedToken.User.IsSystemAdministrator);

        await context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(accessToken, newPlainToken);
    }

    private async Task RevokeAllUserTokens(Guid userId, string triggeredByTokenHash, CancellationToken cancellationToken)
    {
        List<RefreshToken> activeTokens = await context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByTokenHash = triggeredByTokenHash;
        }
    }
}
