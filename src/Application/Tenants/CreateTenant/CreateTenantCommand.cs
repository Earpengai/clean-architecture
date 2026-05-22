using Application.Abstractions.Messaging;

namespace Application.Tenants.CreateTenant;

public sealed record CreateTenantCommand(string Name, string Identifier, Guid OwnerId) : ICommand<CreateTenantResponse>;
