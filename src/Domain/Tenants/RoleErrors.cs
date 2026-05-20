using SharedKernel;

namespace Domain.Tenants;

public static class RoleErrors
{
    public static readonly Error NameNotUnique = Error.Conflict(
        "Roles.NameNotUnique",
        "A role with this name already exists in the tenant");

    public static Error NotFound(Guid roleId) => Error.NotFound(
        "Roles.NotFound",
        $"The role with the Id = '{roleId}' was not found");

    public static readonly Error CannotDeleteSystemRole = Error.Problem(
        "Roles.CannotDeleteSystemRole",
        "System roles cannot be deleted");

    public static readonly Error CannotModifySystemRole = Error.Problem(
        "Roles.CannotModifySystemRole",
        "System role permissions cannot be modified");
}
