namespace Application.Users.GetTenantUserById;

public sealed record UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public bool EmailConfirmed { get; init; }
    public DateTime CreatedAt { get; init; }
    public string RoleName { get; init; }
    public Guid RoleId { get; init; }
}
