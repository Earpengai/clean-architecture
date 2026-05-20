using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, string>
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 15;

    public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        if (user.LockoutEnd is not null && user.LockoutEnd > DateTime.UtcNow)
        {
            return Result.Failure<string>(UserErrors.AccountLocked);
        }

        if (user.LockoutEnd is not null && user.LockoutEnd <= DateTime.UtcNow)
        {
            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
        }

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        List<string> tenantIdentifiers = await context.Memberships
            .Where(m => m.UserId == user.Id)
            .Join(context.Tenants,
                m => m.TenantId,
                t => t.Id,
                (m, t) => t.Identifier)
            .ToListAsync(cancellationToken);

        string token = tokenProvider.Create(user, tenantIdentifiers, user.IsSystemAdministrator);

        await context.SaveChangesAsync(cancellationToken);

        return token;
    }
}
