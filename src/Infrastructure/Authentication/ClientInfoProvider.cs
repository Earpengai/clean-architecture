using System.Linq;
using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class ClientInfoProvider : IClientInfoProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientInfoProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? IpAddress
    {
        get
        {
            HttpContext? context = _httpContextAccessor.HttpContext;

            if (context is null)
            {
                return null;
            }

            string? forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (forwardedFor is not null)
            {
                string firstIp = forwardedFor.Split(',')[0].Trim();

                if (firstIp.Length > 0)
                {
                    return firstIp;
                }
            }

            string? realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();

            if (realIp is not null && realIp.Length > 0)
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
}
