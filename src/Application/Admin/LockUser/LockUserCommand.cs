using Application.Abstractions.Messaging;

namespace Application.Admin.LockUser;

public sealed record LockUserCommand(Guid UserId) : ICommand;
