using SharedKernel;

namespace Domain.Users;

public sealed class RegistrationInfo : Entity
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public bool AcceptedTerms { get; set; }
    public DateTime RegisteredAt { get; set; }
}
