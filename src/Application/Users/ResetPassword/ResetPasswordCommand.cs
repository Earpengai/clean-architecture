using Application.Abstractions.Messaging;

namespace Application.Users.ResetPassword;

public sealed record ResetPasswordCommand(Guid UserId, string Token, string NewPassword) : ICommand;
