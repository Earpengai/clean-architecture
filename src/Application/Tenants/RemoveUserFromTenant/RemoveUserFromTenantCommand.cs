using Application.Abstractions.Messaging;

namespace Application.Tenants.RemoveUserFromTenant;

public sealed record RemoveUserFromTenantCommand(Guid UserId) : ICommand;
