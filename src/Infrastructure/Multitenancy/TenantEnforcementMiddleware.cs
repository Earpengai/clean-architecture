using System.Text.Json;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Multitenancy;

internal sealed class TenantEnforcementMiddleware(RequestDelegate next)
{
    private const string TenantDisabledType = "https://api.example.com/errors/tenant-disabled";
    private const string TenantExpiredType = "https://api.example.com/errors/tenant-expired";
    private const string TrialExpiredType = "https://api.example.com/errors/trial-expired";

    public async Task Invoke(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IExcludeFromMultiTenantResolutionMetadata>() is not null)
        {
            await next(context);
            return;
        }

        AppTenantInfo? tenantInfo = context.GetMultiTenantContext<AppTenantInfo>()?.TenantInfo;

        if (tenantInfo is null)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest,
                "Tenant Required", "The X-Tenant-Id header is required for this endpoint.",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1");
            return;
        }

        if (!tenantInfo.IsActive)
        {
            await WriteProblem(context, StatusCodes.Status403Forbidden,
                "Tenant Disabled", "This tenant has been disabled. Please contact your administrator.",
                TenantDisabledType);
            return;
        }

        if (string.Equals(tenantInfo.SubscriptionStatus, "Expired", StringComparison.OrdinalIgnoreCase))
        {
            await WriteProblem(context, StatusCodes.Status403Forbidden,
                "Subscription Expired", "Your subscription has expired. Please renew to regain access.",
                TenantExpiredType);
            return;
        }

        if (tenantInfo.ExpiresAt is not null && tenantInfo.ExpiresAt.Value < DateTime.UtcNow)
        {
            bool isTrialing = string.Equals(tenantInfo.SubscriptionStatus, "Trialing", StringComparison.OrdinalIgnoreCase);
            string title = isTrialing ? "Trial Expired" : "Subscription Expired";
            string detail = isTrialing
                ? "Your trial period has ended. Please upgrade to a paid plan to continue."
                : "Your subscription has expired. Please renew to regain access.";
            string type = isTrialing ? TrialExpiredType : TenantExpiredType;

            await WriteProblem(context, StatusCodes.Status403Forbidden, title, detail, type);
            return;
        }

        await next(context);
    }

    private static async Task WriteProblem(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string? type = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        Dictionary<string, object?> problem = new()
        {
            ["title"] = title,
            ["status"] = statusCode,
            ["detail"] = detail,
            ["instance"] = context.Request.Path.ToString()
        };

        if (type is not null)
        {
            problem["type"] = type;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
