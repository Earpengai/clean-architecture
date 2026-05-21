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
    UserManager<User> userManager)
    : ICommandHandler<AcceptInvitationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AcceptInvitationCommand command, CancellationToken cancellationToken)
    {
        Invitation? invitation = await context.Invitations
            .Include(i => i.Role)
            .FirstOrDefaultAsync(i => i.Token == command.Token, cancellationToken);

        if (invitation is null)
        {
            return Result.Failure<Guid>(InvitationErrors.NotFound);
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Failure<Guid>(InvitationErrors.AlreadyAccepted);
        }

        if (invitation.TokenExpiry < DateTime.UtcNow)
        {
            return Result.Failure<Guid>(InvitationErrors.Expired);
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
                return Result.Failure<Guid>(InvitationErrors.EmailAlreadyMember);
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
                return Result.Failure<Guid>(UserErrors.FromIdentityResult(result));
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

        return user.Id;
    }
}
