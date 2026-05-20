using Application.Abstractions.Messaging;

namespace Application.Admin.EnableTenant;

public sealed record EnableTenantCommand(Guid TenantId) : ICommand;
