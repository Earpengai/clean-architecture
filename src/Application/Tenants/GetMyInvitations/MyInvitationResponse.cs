namespace Application.Tenants.GetMyInvitations;

public sealed record MyInvitationResponse
{
    public Guid Id { get; init; }
    public string TenantName { get; init; }
    public string RoleName { get; init; }
    public string Token { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime TokenExpiry { get; init; }
}
