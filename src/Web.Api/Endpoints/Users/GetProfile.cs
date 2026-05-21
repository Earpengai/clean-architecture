using Application.Abstractions.Messaging;
using Application.Users.GetCurrentUserProfile;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/profile", async (
            IQueryHandler<GetCurrentUserProfileQuery, UserProfileResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCurrentUserProfileQuery();

            Result<UserProfileResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
