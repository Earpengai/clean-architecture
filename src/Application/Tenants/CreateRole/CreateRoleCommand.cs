using Application.Abstractions.Messaging;

namespace Application.Tenants.CreateRole;

public sealed record CreateRoleCommand(string Name, string? Description, List<string> Permissions) : ICommand<Guid>;
