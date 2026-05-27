using Application.Abstractions.Data;
using Domain.Payments;
using Domain.Subscriptions;
using Domain.Tenants;
using Domain.Todos;
using Domain.Users;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Infrastructure.DomainEvents;
using Infrastructure.Jobs;
using Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher,
    IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor)
    : IdentityDbContext<User, Role, Guid>(options), IApplicationDbContext, IMultiTenantDbContext
{
    public ITenantInfo? TenantInfo { get; } = multiTenantContextAccessor.MultiTenantContext?.TenantInfo;

    public TenantMismatchMode TenantMismatchMode => TenantMismatchMode.Throw;

    public TenantNotSetMode TenantNotSetMode => TenantNotSetMode.Overwrite;

    private Guid? CurrentTenantId
    {
        get
        {
            ITenantInfo? tenantInfo = TenantInfo;

            return tenantInfo is not null ? Guid.Parse(tenantInfo.Id!) : null;
        }
    }

    public DbSet<TodoItem> TodoItems { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Membership> Memberships { get; set; }

    public DbSet<RolePermission> RolePermissions { get; set; }

    public DbSet<Invitation> Invitations { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<TwoFactorRememberToken> TwoFactorRememberTokens { get; set; }

    public DbSet<UserSession> UserSessions { get; set; }

    public DbSet<PlanFeature> PlanFeatures { get; set; }

    public DbSet<PlanLimit> PlanLimits { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<BackgroundJob> BackgroundJobs { get; set; }

    public DbSet<RegistrationInfo> RegistrationInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.HasDefaultSchema(Schemas.Default);

        builder.Entity<TodoItem>().HasQueryFilter(
            t => CurrentTenantId == null || t.TenantId == CurrentTenantId);
        builder.Entity<Membership>().HasQueryFilter(
            m => CurrentTenantId == null || m.TenantId == CurrentTenantId);
        builder.Entity<Invitation>().HasQueryFilter(
            i => CurrentTenantId == null || i.TenantId == CurrentTenantId);
        builder.Entity<Role>().HasQueryFilter(
            r => CurrentTenantId == null || r.TenantId == CurrentTenantId);
        builder.Entity<Payment>().HasQueryFilter(
            p => CurrentTenantId == null || p.TenantId == CurrentTenantId);
        builder.Entity<Subscription>().HasQueryFilter(
            s => CurrentTenantId == null || s.TenantId == CurrentTenantId);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.EnforceMultiTenant();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.EnforceMultiTenant();

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
