using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Tenants.GetMyInvitations;

public sealed record GetMyInvitationsQuery : IQuery<List<MyInvitationResponse>>;
