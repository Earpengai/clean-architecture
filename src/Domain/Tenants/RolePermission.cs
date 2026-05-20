namespace Domain.Tenants;

public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    public string Permission { get; set; }
}
