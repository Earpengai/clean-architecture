using Application.Abstractions.Messaging;
using Application.Users.Register;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class Register : IEndpoint
{
    public sealed record Request(string Email, string FirstName, string LastName, string Password, string ConfirmPassword,
        string CompanyName, string Industry, string Country, bool AcceptedTerms);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (
            Request request,
            ICommandHandler<RegisterUserCommand, RegisterResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password,
                request.ConfirmPassword,
                request.CompanyName,
                request.Industry,
                request.Country,
                request.AcceptedTerms);

            Result<RegisterResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Auth);
    }
}
