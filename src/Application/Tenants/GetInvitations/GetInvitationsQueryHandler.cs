using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.GetInvitations;

internal sealed class GetInvitationsQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetInvitationsQuery, List<InvitationResponse>>
{
    public async Task<Result<List<InvitationResponse>>> Handle(
        GetInvitationsQuery query,
        CancellationToken cancellationToken)
    {
        List<InvitationResponse> invitations = await context.Invitations
            .Where(i => i.TenantId == userContext.TenantId!.Value)
            .Join(context.Roles,
                i => i.RoleId,
                r => r.Id,
                (i, r) => new InvitationResponse
                {
                    Id = i.Id,
                    Email = i.Email,
                    RoleName = r.Name!,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt,
                    TokenExpiry = i.TokenExpiry
                })
            .ToListAsync(cancellationToken);

        return invitations;
    }
}
