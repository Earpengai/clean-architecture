using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.RevokeSession;

internal sealed class RevokeSessionCommandHandler(
    IUserContext userContext,
    IApplicationDbContext context) : ICommandHandler<RevokeSessionCommand>
{
    public async Task<Result> Handle(RevokeSessionCommand command, CancellationToken cancellationToken)
    {
        UserSession? session = await context.UserSessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId && s.IsActive, cancellationToken);

        if (session is null)
        {
            return Result.Failure(UserErrors.SessionNotFound(command.SessionId));
        }

        if (session.UserId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        session.IsActive = false;
        session.RevokedAt = DateTime.UtcNow;

        if (session.RefreshTokenId is not null)
        {
            await context.RefreshTokens
                .Where(r => r.Id == session.RefreshTokenId.Value && r.RevokedAt == null)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(r => r.RevokedAt, DateTime.UtcNow),
                    cancellationToken);
        }

        // Revoke all remember-me tokens for this user — any session termination
        // should require full 2FA on the next login from every device.
        List<TwoFactorRememberToken> rememberTokens = await context.TwoFactorRememberTokens
            .Where(t => t.UserId == session.UserId)
            .ToListAsync(cancellationToken);

        context.TwoFactorRememberTokens.RemoveRange(rememberTokens);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
