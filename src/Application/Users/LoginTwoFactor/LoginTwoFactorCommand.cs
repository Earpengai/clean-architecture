using Application.Abstractions.Messaging;
using Application.Users.Login;

namespace Application.Users.LoginTwoFactor;

public sealed record LoginTwoFactorCommand(Guid UserId, string Code, bool RememberDevice) : ICommand<LoginResponse>;
