using SharedKernel;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");

    public static readonly Error EmailAlreadyVerified = Error.Conflict(
        "Users.EmailAlreadyVerified",
        "The email is already verified");

    public static readonly Error InvalidVerificationToken = Error.Problem(
        "Users.InvalidVerificationToken",
        "The verification token is invalid or has expired");

    public static readonly Error InvalidResetToken = Error.Problem(
        "Users.InvalidResetToken",
        "The password reset token is invalid or has expired");

    public static readonly Error InvalidCredentials = Error.Problem(
        "Users.InvalidCredentials",
        "The current password is incorrect");

    public static readonly Error InvalidInvitationToken = Error.Problem(
        "Users.InvalidInvitationToken",
        "The invitation token is invalid or has expired");

    public static readonly Error NotMemberOfTenant = Error.Problem(
        "Users.NotMemberOfTenant",
        "The user is not a member of this tenant");
}
