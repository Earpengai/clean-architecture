using System.Text.Json;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Multitenancy;

internal sealed class TenantEnforcementMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IExcludeFromMultiTenantResolutionMetadata>() is null)
        {
            AppTenantInfo? tenantInfo = context.GetMultiTenantContext<AppTenantInfo>()?.TenantInfo;

            if (tenantInfo is null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/problem+json";

                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Tenant Required",
                    status = StatusCodes.Status400BadRequest,
                    detail = "The X-Tenant-Id header is required for this endpoint.",
                    instance = context.Request.Path.ToString()
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                return;
            }
        }

        await next(context);
    }
}
