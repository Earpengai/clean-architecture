using Application.Abstractions.Messaging;
using Application.Users.Login;

namespace Application.Users.LoginRecovery;

public sealed record LoginRecoveryCommand(Guid UserId, string RecoveryCode) : ICommand<LoginResponse>;
