using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.InviteUser;

internal sealed class InviteUserCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<InviteUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(InviteUserCommand command, CancellationToken cancellationToken)
    {
        bool alreadyMember = await context.Memberships
            .AnyAsync(m => m.TenantId == userContext.TenantId!.Value
                && m.User != null && m.User.Email == command.Email, cancellationToken);

        if (alreadyMember)
        {
            return Result.Failure<Guid>(InvitationErrors.EmailAlreadyMember);
        }

        Role? role = await context.Roles
            .FirstOrDefaultAsync(r => r.Id == command.RoleId
                && r.TenantId == userContext.TenantId!.Value, cancellationToken);

        if (role is null)
        {
            return Result.Failure<Guid>(RoleErrors.NotFound(command.RoleId));
        }

        Tenant? tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == userContext.TenantId!.Value, cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<Guid>(TenantErrors.NotFound(userContext.TenantId!.Value));
        }

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            TenantId = userContext.TenantId!.Value,
            Email = command.Email,
            RoleId = command.RoleId,
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            TokenExpiry = DateTime.UtcNow.AddDays(7),
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        invitation.Raise(new InvitationCreatedDomainEvent(
            invitation.Id, invitation.Email, invitation.Token, tenant.Name));

        context.Invitations.Add(invitation);

        await context.SaveChangesAsync(cancellationToken);

        return invitation.Id;
    }
}
