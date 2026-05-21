namespace Application.Users.GetCurrentUserProfile;

public sealed record UserProfileResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public bool TwoFactorEnabled { get; init; }

    public bool EmailConfirmed { get; init; }
}
