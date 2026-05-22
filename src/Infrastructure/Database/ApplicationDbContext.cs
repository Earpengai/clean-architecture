using Application.Abstractions.Data;
using Domain.Tenants;
using Domain.Todos;
using Domain.Users;
using Infrastructure.DomainEvents;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : IdentityDbContext<User, Role, Guid>(options), IApplicationDbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Membership> Memberships { get; set; }

    public DbSet<RolePermission> RolePermissions { get; set; }

    public DbSet<Invitation> Invitations { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<PlanFeature> PlanFeatures { get; set; }

    public DbSet<PlanLimit> PlanLimits { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<IDomainEvent> domainEvents = ExtractDomainEvents();
        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync(domainEvents);

        return result;
    }

    private async Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        await domainEventsDispatcher.DispatchAsync(domainEvents);
    }

    private List<IDomainEvent> ExtractDomainEvents()
    {
        var domainEvents = ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IDomainEventSource>()
            .SelectMany(source =>
            {
                List<IDomainEvent> events = source.DomainEvents;

                source.ClearDomainEvents();

                return events;
            })
            .ToList();
        return domainEvents;
    }
}
