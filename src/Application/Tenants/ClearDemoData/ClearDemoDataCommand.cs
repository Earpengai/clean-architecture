using Application.Abstractions.Messaging;

namespace Application.Tenants.ClearDemoData;

public sealed record ClearDemoDataCommand(Guid TenantId) : ICommand;
