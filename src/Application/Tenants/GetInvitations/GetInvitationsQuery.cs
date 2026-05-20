using Application.Abstractions.Messaging;

namespace Application.Tenants.GetInvitations;

public sealed record GetInvitationsQuery : IQuery<List<InvitationResponse>>;
