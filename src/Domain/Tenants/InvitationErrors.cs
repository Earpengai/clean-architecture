using SharedKernel;

namespace Domain.Tenants;

public static class InvitationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Invitations.NotFound",
        "The invitation was not found");

    public static readonly Error AlreadyAccepted = Error.Conflict(
        "Invitations.AlreadyAccepted",
        "This invitation has already been accepted");

    public static readonly Error Expired = Error.Problem(
        "Invitations.Expired",
        "This invitation has expired");

    public static readonly Error Canceled = Error.Problem(
        "Invitations.Canceled",
        "This invitation has been canceled");

    public static readonly Error EmailAlreadyMember = Error.Conflict(
        "Invitations.EmailAlreadyMember",
        "This email is already a member of the tenant");
}
