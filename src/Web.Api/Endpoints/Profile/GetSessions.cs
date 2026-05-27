using Application.Abstractions.Messaging;
using Application.Users.GetUserSessions;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profile;

internal sealed class GetSessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profile/sessions", async (
            IQueryHandler<GetUserSessionsQuery, List<UserSessionResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserSessionsQuery();

            Result<List<UserSessionResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Profile)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
