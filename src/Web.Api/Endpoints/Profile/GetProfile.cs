using Application.Abstractions.Messaging;
using Application.Users.GetCurrentUserProfile;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profile;

internal sealed class GetProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profile", async (
            IQueryHandler<GetCurrentUserProfileQuery, UserProfileResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCurrentUserProfileQuery();

            Result<UserProfileResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Profile)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
