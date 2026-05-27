using Application.Abstractions.Messaging;
using Application.Users.Login;
using Application.Users.LoginTwoFactor;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class LoginTwoFactor : IEndpoint
{
    public sealed record Request(Guid UserId, string Code, bool RememberDevice);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login-2fa", async (
            Request request,
            HttpContext httpContext,
            ICommandHandler<LoginTwoFactorCommand, LoginResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginTwoFactorCommand(request.UserId, request.Code, request.RememberDevice);

            Result<LoginResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(value =>
            {
                if (value.RememberDeviceToken is not null)
                {
                    httpContext.Response.Cookies.Append("two_factor_remember", value.RememberDeviceToken,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            MaxAge = TimeSpan.FromDays(30)
                        });
                }

                return Results.Ok(value);
            }, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Auth);
    }
}
