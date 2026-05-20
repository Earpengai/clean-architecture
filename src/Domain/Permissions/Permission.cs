namespace Domain.Permissions;

public static class Permission
{
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersDelete = "users:delete";
    public const string RolesRead = "roles:read";
    public const string RolesWrite = "roles:write";
    public const string RolesDelete = "roles:delete";
    public const string TenantsRead = "tenants:read";
    public const string TenantsWrite = "tenants:write";
    public const string TenantsDelete = "tenants:delete";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        UsersRead, UsersWrite, UsersDelete,
        RolesRead, RolesWrite, RolesDelete,
        TenantsRead, TenantsWrite, TenantsDelete
    };
}
