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

internal sealed class AcceptInvitationCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    ITokenProvider tokenProvider)
    : ICommandHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{
    public async Task<Result<AcceptInvitationResponse>> Handle(AcceptInvitationCommand command, CancellationToken cancellationToken)
    {
        Invitation? invitation = await context.Invitations
            .Include(i => i.Role)
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

        User? existingUser = await userManager.FindByEmailAsync(invitation.Email);

        User user;
        if (existingUser is not null)
        {
            bool alreadyMember = await context.Memberships
                .AnyAsync(m => m.UserId == existingUser.Id
                    && m.TenantId == invitation.TenantId, cancellationToken);

            if (alreadyMember)
            {
                return Result.Failure<AcceptInvitationResponse>(InvitationErrors.EmailAlreadyMember);
            }

            user = existingUser;
        }
        else
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = invitation.Email,
                UserName = invitation.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            IdentityResult result = await userManager.CreateAsync(user, command.Password);

            if (!result.Succeeded)
            {
                return Result.Failure<AcceptInvitationResponse>(UserErrors.FromIdentityResult(result));
            }
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
