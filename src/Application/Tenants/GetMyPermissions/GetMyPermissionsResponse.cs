namespace Application.Tenants.GetMyPermissions;

public sealed record GetMyPermissionsResponse(IReadOnlySet<string> Permissions);
