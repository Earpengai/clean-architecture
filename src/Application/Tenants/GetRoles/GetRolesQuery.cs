using Application.Abstractions.Messaging;

namespace Application.Tenants.GetRoles;

public sealed record GetRolesQuery : IQuery<List<RoleResponse>>;
