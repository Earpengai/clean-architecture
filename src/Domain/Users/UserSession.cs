using SharedKernel;

namespace Domain.Users;

public sealed class UserSession : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid? RefreshTokenId { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string Browser { get; set; }
    public string OperatingSystem { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
