using Application.Abstractions.Messaging;

namespace Application.Users.GetCurrentUserProfile;

public sealed record GetCurrentUserProfileQuery : IQuery<UserProfileResponse>;
