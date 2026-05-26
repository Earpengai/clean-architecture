using Application.Abstractions.Data;
using Application.Tenants.SeedDemoData;
using Domain.Tenants;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Data;

internal sealed class SeedDemoDataJobHandler(
    IApplicationDbContext context,
    ILogger<SeedDemoDataJobHandler> logger)
    : IBackgroundJobHandler<SeedDemoDataJob>
{
    public async Task Handle(SeedDemoDataJob job, CancellationToken cancellationToken)
    {
        logger.LogDebug("Seeding demo data for tenant {TenantId}", job.TenantId);

        var ownerUserId = Guid.NewGuid();
        Membership? membership = await context.Memberships
            .Where(m => m.TenantId == job.TenantId && m.Role.Name == "Owner")
            .Select(m => new Membership { UserId = m.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (membership is not null)
        {
            ownerUserId = membership.UserId;
        }

        List<TodoItem> demoItems = CreateDemoTodoItems(job.TenantId, ownerUserId);

        context.TodoItems.AddRange(demoItems);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogDebug("Seeded {Count} demo todo items for tenant {TenantId}", demoItems.Count, job.TenantId);
    }

    private static List<TodoItem> CreateDemoTodoItems(Guid tenantId, Guid userId)
    {
        DateTime now = DateTime.UtcNow;

        List<TodoItem> items = [];

        AddCompletedOneMonthAgo(items, tenantId, userId, now);
        AddCompletedPastWeek(items, tenantId, userId, now);
        AddDueThisWeek(items, tenantId, userId, now);
        AddDueComingWeeks(items, tenantId, userId, now);
        AddBacklog(items, tenantId, userId, now);
        AddHierarchies(items, tenantId, userId, now);

        return items;
    }

    private static TodoItem CreateItem(
        Guid tenantId,
        Guid userId,
        string description,
        DateTime createdAt,
        Priority priority = Priority.Normal,
        string[]? labels = null,
        DateTime? dueDate = null,
        bool isCompleted = false,
        DateTime? completedAt = null,
        Guid? parentId = null)
    {
        return new TodoItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            ParentId = parentId,
            Description = description,
            DueDate = dueDate,
            Labels = labels is not null ? [.. labels] : [],
            IsCompleted = isCompleted,
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            Priority = priority
        };
    }

    private static void AddCompletedOneMonthAgo(List<TodoItem> items, Guid tenantId, Guid userId, DateTime now)
    {
        DateTime created = now.AddDays(-35);
        DateTime completed = now.AddDays(-30);

        items.AddRange([
            CreateItem(tenantId, userId, "Upgrade authentication library to v3", created, Priority.High,
                ["backend", "security"], dueDate: now.AddDays(-28), isCompleted: true, completedAt: completed),
            CreateItem(tenantId, userId, "Implement rate limiting on all API endpoints", created.AddDays(1), Priority.High,
                ["backend", "devops"], dueDate: now.AddDays(-26), isCompleted: true, completedAt: completed),
            CreateItem(tenantId, userId, "Add CSV export for analytics dashboard", created.AddDays(2), Priority.Medium,
                ["feature", "frontend"], dueDate: now.AddDays(-25), isCompleted: true, completedAt: completed.AddDays(1)),
            CreateItem(tenantId, userId, "Fix pagination bug on search results", created.AddDays(3), Priority.Top,
                ["bug", "backend"], dueDate: now.AddDays(-24), isCompleted: true, completedAt: completed.AddDays(1)),
            CreateItem(tenantId, userId, "Write unit tests for payment processing module", created.AddDays(4), Priority.High,
                ["testing", "backend"], dueDate: now.AddDays(-22), isCompleted: true, completedAt: completed.AddDays(2)),
            CreateItem(tenantId, userId, "Migrate legacy user preferences to new schema", created.AddDays(5), Priority.Medium,
                ["backend", "data"], dueDate: now.AddDays(-20), isCompleted: true, completedAt: completed.AddDays(2)),
            CreateItem(tenantId, userId, "Set up automated database backups", created.AddDays(6), Priority.High,
                ["devops", "data"], dueDate: now.AddDays(-18), isCompleted: true, completedAt: completed.AddDays(3)),
            CreateItem(tenantId, userId, "Add dark mode toggle to settings page", created.AddDays(7), Priority.Medium,
                ["feature", "frontend"], dueDate: now.AddDays(-15), isCompleted: true, completedAt: completed.AddDays(3)),
            CreateItem(tenantId, userId, "Refactor notification service to use message queue", created.AddDays(8), Priority.High,
                ["backend", "devops"], dueDate: now.AddDays(-12), isCompleted: true, completedAt: completed.AddDays(4)),
            CreateItem(tenantId, userId, "Update README and contribution guidelines", created.AddDays(9), Priority.Low,
                ["documentation"], dueDate: now.AddDays(-10), isCompleted: true, completedAt: completed.AddDays(4)),
        ]);
    }

    private static void AddCompletedPastWeek(List<TodoItem> items, Guid tenantId, Guid userId, DateTime now)
    {
        DateTime created = now.AddDays(-10);

        items.AddRange([
            CreateItem(tenantId, userId, "Resolve N+1 query in user listing endpoint", created, Priority.Top,
                ["bug", "backend", "performance"], dueDate: now.AddDays(-3), isCompleted: true, completedAt: now.AddDays(-2)),
            CreateItem(tenantId, userId, "Update terms of service page", created.AddDays(1), Priority.Low,
                ["documentation", "frontend"], dueDate: now.AddDays(-1), isCompleted: true, completedAt: now.AddDays(-1)),
            CreateItem(tenantId, userId, "Add email verification resend cooldown", created.AddDays(2), Priority.Medium,
                ["feature", "security"], dueDate: now.AddDays(-2), isCompleted: true, completedAt: now.AddDays(-3)),
            CreateItem(tenantId, userId, "Implement soft delete for archived projects", created.AddDays(2), Priority.High,
                ["feature", "backend"], dueDate: now.AddDays(-1), isCompleted: true, completedAt: now.AddDays(-1)),
            CreateItem(tenantId, userId, "Cache frequently accessed reference data", created.AddDays(3), Priority.Medium,
                ["backend", "performance"], dueDate: now.AddDays(-4), isCompleted: true, completedAt: now.AddDays(-4)),
            CreateItem(tenantId, userId, "Fix mobile layout for onboarding wizard", created.AddDays(4), Priority.High,
                ["bug", "frontend"], dueDate: now.AddDays(-2), isCompleted: true, completedAt: now.AddDays(-2)),
            CreateItem(tenantId, userId, "Add input sanitization on user-generated content", created.AddDays(5), Priority.High,
                ["security", "backend"], dueDate: now.AddDays(-1), isCompleted: true, completedAt: now.AddDays(-1)),
            CreateItem(tenantId, userId, "Review and merge all open dependabot PRs", created.AddDays(6), Priority.Medium,
                ["devops"], dueDate: now.AddDays(-3), isCompleted: true, completedAt: now.AddDays(-3)),
        ]);
    }

    private static void AddDueThisWeek(List<TodoItem> items, Guid tenantId, Guid userId, DateTime now)
    {
        items.AddRange([
            CreateItem(tenantId, userId, "Optimize image loading with lazy loading", now.AddDays(-1), Priority.High,
                ["frontend", "performance"], dueDate: now.AddDays(1)),
            CreateItem(tenantId, userId, "Fix session timeout redirect loop", now.AddDays(-2), Priority.Top,
                ["bug", "security"], dueDate: now),
            CreateItem(tenantId, userId, "Add tooltip descriptions to dashboard widgets", now.AddDays(-3), Priority.Low,
                ["frontend", "feature"], dueDate: now.AddDays(2)),
            CreateItem(tenantId, userId, "Implement bulk user import via CSV", now.AddDays(-1), Priority.High,
                ["feature", "backend"], dueDate: now.AddDays(3)),
            CreateItem(tenantId, userId, "Update dependencies with critical CVEs", now, Priority.Top,
                ["security", "devops"], dueDate: now.AddDays(1)),
            CreateItem(tenantId, userId, "Add audit logging for role changes", now.AddDays(-2), Priority.Medium,
                ["feature", "backend", "security"], dueDate: now.AddDays(2)),
            CreateItem(tenantId, userId, "Create onboarding email sequence templates", now.AddDays(-4), Priority.Medium,
                ["feature", "documentation"], dueDate: now.AddDays(4)),
            CreateItem(tenantId, userId, "Fix date picker timezone conversion", now.AddDays(-1), Priority.High,
                ["bug", "frontend"], dueDate: now.AddDays(1)),
            CreateItem(tenantId, userId, "Add request tracing correlation IDs", now.AddDays(-3), Priority.Medium,
                ["backend", "devops"], dueDate: now.AddDays(3)),
            CreateItem(tenantId, userId, "Stress test API under 10k concurrent users", now.AddDays(-5), Priority.High,
                ["testing", "devops"], dueDate: now.AddDays(5)),
        ]);
    }

    private static void AddDueComingWeeks(List<TodoItem> items, Guid tenantId, Guid userId, DateTime now)
    {
        items.AddRange([
            CreateItem(tenantId, userId, "Build notification preferences center", now, Priority.High,
                ["feature", "frontend", "backend"], dueDate: now.AddDays(14)),
            CreateItem(tenantId, userId, "Migrate file storage to S3-compatible bucket", now.AddDays(-2), Priority.High,
                ["backend", "devops", "data"], dueDate: now.AddDays(21)),
            CreateItem(tenantId, userId, "Implement webhook subscriptions for third-party integrations", now.AddDays(-1), Priority.Medium,
                ["feature", "backend"], dueDate: now.AddDays(28)),
            CreateItem(tenantId, userId, "Add multi-language support (i18n)", now.AddDays(-3), Priority.Medium,
                ["feature", "frontend"], dueDate: now.AddDays(30)),
            CreateItem(tenantId, userId, "Redesign landing page based on user feedback", now, Priority.Low,
                ["design", "frontend"], dueDate: now.AddDays(18)),
            CreateItem(tenantId, userId, "Set up staging environment with production parity", now.AddDays(-1), Priority.High,
                ["devops"], dueDate: now.AddDays(14)),
            CreateItem(tenantId, userId, "Build interactive API playground (Swagger UI)", now.AddDays(-4), Priority.Medium,
                ["feature", "documentation", "frontend"], dueDate: now.AddDays(21)),
            CreateItem(tenantId, userId, "Implement JWT token rotation on password change", now.AddDays(-2), Priority.High,
                ["security", "backend"], dueDate: now.AddDays(16)),
            CreateItem(tenantId, userId, "Add scheduled report generation (weekly/monthly)", now.AddDays(-1), Priority.Medium,
                ["feature", "backend"], dueDate: now.AddDays(25)),
            CreateItem(tenantId, userId, "Conduct security penetration testing", now.AddDays(-6), Priority.Top,
                ["security", "testing"], dueDate: now.AddDays(28)),
        ]);
    }

    private static void AddBacklog(List<TodoItem> items, Guid tenantId, Guid userId, DateTime now)
    {
        items.AddRange([
            CreateItem(tenantId, userId, "Explore GraphQL as alternative to REST", now.AddDays(-7), Priority.Low,
                ["research", "backend"]),
            CreateItem(tenantId, userId, "Investigate cost optimization for cloud infrastructure", now.AddDays(-5), Priority.Low,
                ["devops", "research"]),
            CreateItem(tenantId, userId, "Evaluate headless CMS options for blog integration", now.AddDays(-3), Priority.Low,
                ["research", "feature"]),
            CreateItem(tenantId, userId, "Create user persona documentation", now.AddDays(-10), Priority.Low,
                ["documentation", "design"]),
            CreateItem(tenantId, userId, "Prototype real-time collaboration feature", now.AddDays(-8), Priority.Medium,
                ["research", "feature"]),
        ]);
    }

    private static void AddHierarchies(List<TodoItem> items, Guid tenantId, Guid userId, DateTime now)
    {
        TodoItem q3Roadmap = CreateItem(tenantId, userId, "Plan Q3 product roadmap", now.AddDays(-5),
            Priority.High, ["planning", "feature"], dueDate: now.AddDays(14));
        items.Add(q3Roadmap);

        items.AddRange([
            CreateItem(tenantId, userId, "Research competitor features", now.AddDays(-4),
                Priority.Normal, ["research"], dueDate: now.AddDays(3), isCompleted: true, completedAt: now.AddHours(2), parentId: q3Roadmap.Id),
            CreateItem(tenantId, userId, "Draft feature specifications", now.AddDays(-3),
                Priority.Medium, ["documentation"], dueDate: now.AddDays(7), parentId: q3Roadmap.Id),
            CreateItem(tenantId, userId, "Estimate development effort per feature", now.AddDays(-2),
                Priority.Medium, ["planning"], dueDate: now.AddDays(10), parentId: q3Roadmap.Id),
        ]);

        TodoItem onboardingRevamp = CreateItem(tenantId, userId, "Revamp user onboarding flow", now.AddDays(-6),
            Priority.High, ["feature", "frontend", "design"], dueDate: now.AddDays(20));
        items.Add(onboardingRevamp);

        items.AddRange([
            CreateItem(tenantId, userId, "Design new welcome screen mockups", now.AddDays(-5),
                Priority.High, ["design", "frontend"], dueDate: now.AddDays(5), parentId: onboardingRevamp.Id),
            CreateItem(tenantId, userId, "Implement step-by-step guided tour", now.AddDays(-3),
                Priority.Medium, ["feature", "frontend"], dueDate: now.AddDays(15), parentId: onboardingRevamp.Id),
            CreateItem(tenantId, userId, "Add progress tracking to onboarding checklist", now.AddDays(-1),
                Priority.Medium, ["feature", "frontend"], dueDate: now.AddDays(18), parentId: onboardingRevamp.Id),
        ]);

        TodoItem mobileApp = CreateItem(tenantId, userId, "Develop companion mobile application", now.AddDays(-15),
            Priority.Top, ["feature", "frontend", "design"], dueDate: now.AddDays(45));
        items.Add(mobileApp);

        items.AddRange([
            CreateItem(tenantId, userId, "Design mobile app wireframes", now.AddDays(-14),
                Priority.High, ["design"], dueDate: now.AddDays(-7), isCompleted: true, completedAt: now.AddDays(-8), parentId: mobileApp.Id),
            CreateItem(tenantId, userId, "Set up React Native project with shared types", now.AddDays(-10),
                Priority.High, ["frontend", "devops"], dueDate: now.AddDays(5), parentId: mobileApp.Id),
            CreateItem(tenantId, userId, "Implement authentication flow for mobile", now.AddDays(-2),
                Priority.High, ["frontend", "security", "backend"], dueDate: now.AddDays(20), parentId: mobileApp.Id),
        ]);
    }
}
