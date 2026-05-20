using Application.Abstractions.Messaging;

namespace Application.Users.UpdateProfile;

public sealed record UpdateUserProfileCommand(string FirstName, string LastName) : ICommand;
