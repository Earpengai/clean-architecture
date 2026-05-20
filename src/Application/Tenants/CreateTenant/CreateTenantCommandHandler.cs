using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTenantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        bool identifierExists = await context.Tenants
            .AnyAsync(t => t.Identifier == command.Identifier, cancellationToken);

        if (identifierExists)
        {
            return Result.Failure<Guid>(TenantErrors.IdentifierNotUnique);
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Identifier = command.Identifier,
            SubscriptionPlan = SubscriptionPlan.Free,
            SubscriptionStatus = SubscriptionStatus.Trialing,
            SeatCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        var membership = new Membership
        {
            UserId = command.OwnerId,
            TenantId = tenant.Id,
            Role = MembershipRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        context.Tenants.Add(tenant);
        context.Memberships.Add(membership);

        await context.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
