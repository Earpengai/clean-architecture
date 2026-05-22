namespace Application.Tenants.CreateTenant;

public sealed record CreateTenantResponse(Guid TenantId, string AccessToken, string RefreshToken);
