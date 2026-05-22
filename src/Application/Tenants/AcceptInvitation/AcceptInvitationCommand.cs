using Application.Abstractions.Messaging;

namespace Application.Tenants.AcceptInvitation;

public sealed record AcceptInvitationCommand(string Token, string FirstName, string LastName, string Password) : ICommand<AcceptInvitationResponse>;
