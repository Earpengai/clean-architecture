using Application.Abstractions.Messaging;

namespace Application.Tenants.UpdateRole;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, string? Description, List<string> Permissions) : ICommand;
