using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.GetMyPermissions;

internal sealed class GetMyPermissionsQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetMyPermissionsQuery, GetMyPermissionsResponse>
{
    public async Task<Result<GetMyPermissionsResponse>> Handle(
        GetMyPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        HashSet<string> permissions = await context.Memberships
            .Where(m => m.UserId == userContext.UserId && m.TenantId == userContext.TenantId!.Value)
            .SelectMany(m => m.Role.Permissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToHashSetAsync(cancellationToken);

        return new GetMyPermissionsResponse(permissions);
    }
}
