using Microsoft.AspNetCore.Identity;
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
        "An account with this email already exists.");

    public static readonly Error EmailAlreadyVerified = Error.Conflict(
        "Users.EmailAlreadyVerified",
        "The email is already verified");

    public static readonly Error EmailNotVerified = Error.Problem(
        "Users.EmailNotVerified",
        "Your email address has not been verified. Please check your inbox for the verification link.");

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

    public static readonly Error AccountLocked = Error.Problem(
        "Users.AccountLocked",
        "Your account has been locked due to too many failed login attempts. Please try again later.");

    public static readonly Error PasswordNotCompliant = Error.Problem(
        "Users.PasswordNotCompliant",
        "The new password does not meet the security requirements.");

    public static readonly Error InvalidRecoveryCode = Error.Problem(
        "Users.InvalidRecoveryCode",
        "The recovery code is invalid or has already been used.");

    public static Error SessionNotFound(Guid sessionId) => Error.NotFound(
        "Users.SessionNotFound",
        $"The session with the Id = '{sessionId}' was not found.");

    public static Error FromIdentityResult(IdentityResult result)
    {
        IdentityError? firstError = result.Errors.FirstOrDefault();

        if (firstError is not null)
        {
            return Error.Failure(firstError.Code, firstError.Description);
        }

        return Error.Failure("Users.IdentityError", "An unknown identity error occurred.");
    }
}
