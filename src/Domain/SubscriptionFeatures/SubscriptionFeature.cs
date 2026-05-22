namespace Domain.SubscriptionFeatures;

public static class SubscriptionFeature
{
    public const string ApiAccess = "feature:api_access";
    public const string Reporting = "feature:reporting";
    public const string AuditLog = "feature:audit_log";
    public const string CustomDomain = "feature:custom_domain";
    public const string PrioritySupport = "feature:priority_support";
    public const string Sso = "feature:sso";
    public const string WhiteLabel = "feature:white_label";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        ApiAccess, Reporting, AuditLog,
        CustomDomain, PrioritySupport, Sso, WhiteLabel
    };
}
