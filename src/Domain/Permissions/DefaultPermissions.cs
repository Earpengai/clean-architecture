namespace Domain.Permissions;

public static class DefaultPermissions
{
    public static readonly HashSet<string> Owner = [.. Permission.All];

    public static readonly HashSet<string> Admin =
    [
        Permission.UsersRead,
        Permission.UsersWrite,
        Permission.RolesRead,
        Permission.TenantsRead
    ];

    public static readonly HashSet<string> Member =
    [
        Permission.TenantsRead
    ];
}
