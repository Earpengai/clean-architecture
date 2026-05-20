using Application.Abstractions.Messaging;

namespace Application.Tenants.AssignUserRole;

public sealed record AssignUserRoleCommand(Guid UserId, Guid RoleId) : ICommand;
