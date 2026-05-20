using Application.Abstractions.Messaging;

namespace Application.Admin.DisableTenant;

public sealed record DisableTenantCommand(Guid TenantId) : ICommand;
