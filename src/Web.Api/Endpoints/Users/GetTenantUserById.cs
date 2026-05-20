using Application.Abstractions.Messaging;
using Application.Users.GetTenantUserById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetTenantUserById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}", async (
            Guid userId,
            IQueryHandler<GetTenantUserByIdQuery, UserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantUserByIdQuery(userId);

            Result<UserResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.UsersRead)
        .WithTags(Tags.Users);
    }
}
