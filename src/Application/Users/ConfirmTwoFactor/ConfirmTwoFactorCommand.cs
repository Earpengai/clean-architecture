using Application.Abstractions.Messaging;

namespace Application.Users.ConfirmTwoFactor;

public sealed record ConfirmTwoFactorCommand(string Code) : ICommand<ConfirmTwoFactorResponse>;
