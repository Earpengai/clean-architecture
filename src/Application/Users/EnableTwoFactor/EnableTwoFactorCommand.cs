using Application.Abstractions.Messaging;

namespace Application.Users.EnableTwoFactor;

public sealed record EnableTwoFactorCommand : ICommand<EnableTwoFactorResponse>;
