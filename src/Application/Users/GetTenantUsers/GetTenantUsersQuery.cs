using Application.Abstractions.Messaging;

namespace Application.Users.GetTenantUsers;

public sealed record GetTenantUsersQuery : IQuery<List<UserResponse>>;
