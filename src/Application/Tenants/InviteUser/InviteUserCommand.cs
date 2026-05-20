using Application.Abstractions.Messaging;

namespace Application.Tenants.InviteUser;

public sealed record InviteUserCommand(string Email, Guid RoleId) : ICommand<Guid>;
