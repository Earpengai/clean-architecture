using Application.Abstractions.Messaging;

namespace Application.Users.GetTenantUserById;

public sealed record GetTenantUserByIdQuery(Guid UserId) : IQuery<UserResponse>;
