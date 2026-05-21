using Application.Abstractions.Messaging;

namespace Application.Admin.UnlockUser;

public sealed record UnlockUserCommand(Guid UserId) : ICommand;
