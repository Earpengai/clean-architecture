namespace Application.Users.DomainEventHandlers;

internal static class UserAgentParser
{
    public static string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        if (userAgent.Contains("Edg/"))
        {
            return "Edge";
        }

        if (userAgent.Contains("OPR/") || userAgent.Contains("Opera"))
        {
            return "Opera";
        }

        if (userAgent.Contains("Chrome/"))
        {
            return "Chrome";
        }

        if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome/"))
        {
            return "Safari";
        }

        if (userAgent.Contains("Firefox/"))
        {
            return "Firefox";
        }

        return "Unknown";
    }

    public static string ParseOperatingSystem(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        if (userAgent.Contains("Windows NT"))
        {
            return "Windows";
        }

        if (userAgent.Contains("Mac OS X") || userAgent.Contains("macOS"))
        {
            return "macOS";
        }

        if (userAgent.Contains("Android"))
        {
            return "Android";
        }

        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
        {
            return "iOS";
        }

        if (userAgent.Contains("Linux"))
        {
            return "Linux";
        }

        return "Unknown";
    }
}
