using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.GetMyInvitations;

internal sealed class GetMyInvitationsQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    UserManager<User> userManager)
    : IQueryHandler<GetMyInvitationsQuery, List<MyInvitationResponse>>
{
    public async Task<Result<List<MyInvitationResponse>>> Handle(
        GetMyInvitationsQuery query,
        CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());

        if (user is null)
        {
            return Result.Failure<List<MyInvitationResponse>>(UserErrors.NotFound(userContext.UserId));
        }

        List<MyInvitationResponse> invitations = await context.Invitations
            .Where(i => i.Email == user.Email && i.Status == InvitationStatus.Pending)
            .Join(context.Tenants,
                i => i.TenantId,
                t => t.Id,
                (i, t) => new { Invitation = i, Tenant = t })
            .Join(context.Roles,
                x => x.Invitation.RoleId,
                r => r.Id,
                (x, r) => new MyInvitationResponse
                {
                    Id = x.Invitation.Id,
                    TenantName = x.Tenant.Name,
                    RoleName = r.Name!,
                    Token = x.Invitation.Token,
                    CreatedAt = x.Invitation.CreatedAt,
                    TokenExpiry = x.Invitation.TokenExpiry
                })
            .ToListAsync(cancellationToken);

        return invitations;
    }
}
