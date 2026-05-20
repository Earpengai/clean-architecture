using Application.Abstractions.Messaging;

namespace Application.Users.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand;
