using Application.Abstractions.Messaging;

namespace Application.Tenants.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand;
