using Application.Abstractions.Messaging;

namespace Application.Tenants.AcceptInvitation;

public sealed record AcceptInvitationForUserCommand(string Token) : ICommand<AcceptInvitationResponse>;
