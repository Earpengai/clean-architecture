using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.AcceptInvitation;

internal sealed class AcceptInvitationForUserCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    UserManager<User> userManager,
    ITokenProvider tokenProvider)
    : ICommandHandler<AcceptInvitationForUserCommand, AcceptInvitationResponse>
{
    public async Task<Result<AcceptInvitationResponse>> Handle(
        AcceptInvitationForUserCommand command,
        CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());

        if (user is null)
        {
            return Result.Failure<AcceptInvitationResponse>(UserErrors.NotFound(userContext.UserId));
        }

        Invitation? invitation = await context.Invitations
            .FirstOrDefaultAsync(i => i.Token == command.Token, cancellationToken);

        if (invitation is null)
        {
            return Result.Failure<AcceptInvitationResponse>(InvitationErrors.NotFound);
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Failure<AcceptInvitationResponse>(InvitationErrors.AlreadyAccepted);
        }

        if (invitation.TokenExpiry < DateTime.UtcNow)
        {
            return Result.Failure<AcceptInvitationResponse>(InvitationErrors.Expired);
        }

        if (!string.Equals(user.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AcceptInvitationResponse>(InvitationErrors.EmailMismatch);
        }

        bool alreadyMember = await context.Memberships
            .AnyAsync(m => m.UserId == user.Id
                && m.TenantId == invitation.TenantId, cancellationToken);

        if (alreadyMember)
        {
            return Result.Failure<AcceptInvitationResponse>(InvitationErrors.EmailAlreadyMember);
        }

        var membership = new Membership
        {
            UserId = user.Id,
            TenantId = invitation.TenantId,
            RoleId = invitation.RoleId,
            JoinedAt = DateTime.UtcNow
        };

        context.Memberships.Add(membership);

        invitation.Status = InvitationStatus.Accepted;

        await context.SaveChangesAsync(cancellationToken);

        List<string> tenantIdentifiers = await context.Memberships
            .Where(m => m.UserId == user.Id)
            .Join(context.Tenants,
                m => m.TenantId,
                t => t.Id,
                (m, t) => t.Identifier)
            .ToListAsync(cancellationToken);

        string accessToken = tokenProvider.Create(
            user, tenantIdentifiers, user.IsSystemAdministrator);

        string plainRefreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        string refreshTokenHash = Convert.ToHexString(
            SHA256.HashData(Convert.FromHexString(plainRefreshToken)));

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

        return new AcceptInvitationResponse(user.Id, accessToken, plainRefreshToken);
    }
}
