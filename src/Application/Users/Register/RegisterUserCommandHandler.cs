using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    IConfiguration configuration)
    : ICommandHandler<RegisterUserCommand, RegisterResponse>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return Result.Failure<RegisterResponse>(UserErrors.EmailNotUnique);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            UserName = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            CreatedAt = DateTime.UtcNow
        };

        IdentityResult result = await userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            return Result.Failure<RegisterResponse>(UserErrors.FromIdentityResult(result));
        }

        bool sendVerification = configuration.GetValue<bool>("EmailVerification:SendVerificationEmail");

        var registrationInfo = new RegistrationInfo
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            CompanyName = command.CompanyName,
            Industry = command.Industry,
            Country = command.Country,
            AcceptedTerms = command.AcceptedTerms,
            RegisteredAt = DateTime.UtcNow
        };

        context.RegistrationInfos.Add(registrationInfo);

        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        user.Raise(new UserRegisteredDomainEvent(user.Id, user.Email!, token));

        await context.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, sendVerification);
    }
}
