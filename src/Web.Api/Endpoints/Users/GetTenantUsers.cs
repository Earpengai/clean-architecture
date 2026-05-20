using Application.Abstractions.Messaging;
using Application.Users.GetTenantUsers;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetTenantUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (
            IQueryHandler<GetTenantUsersQuery, List<UserResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantUsersQuery();

            Result<List<UserResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.UsersRead)
        .WithTags(Tags.Users);
    }
}
