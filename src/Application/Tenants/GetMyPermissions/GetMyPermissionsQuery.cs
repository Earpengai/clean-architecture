using Application.Abstractions.Messaging;

namespace Application.Tenants.GetMyPermissions;

public sealed record GetMyPermissionsQuery : IQuery<GetMyPermissionsResponse>;
