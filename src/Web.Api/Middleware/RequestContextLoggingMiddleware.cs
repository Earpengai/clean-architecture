using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Web.Api.Middleware;

public partial class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "Correlation-Id";
    private const int MaxCorrelationIdLength = 64;

    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            return next.Invoke(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out StringValues correlationId);

        string? value = correlationId.FirstOrDefault();

        if (value is not null
            && value.Length <= MaxCorrelationIdLength
            && CorrelationIdRegex().IsMatch(value))
        {
            return value;
        }

        return context.TraceIdentifier;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9\-_]+$")]
    private static partial Regex CorrelationIdRegex();
}
