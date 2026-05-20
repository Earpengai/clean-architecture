using Domain.Tenants;

namespace Application.Tenants.GetInvitations;

public sealed record InvitationResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string RoleName { get; init; }
    public InvitationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime TokenExpiry { get; init; }
}
