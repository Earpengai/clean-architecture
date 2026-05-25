# Developer Guide

Contribution guide and architecture reference for the Clean Architecture template.

## Overview

A multi-tenant SaaS application template built with .NET 10, ASP.NET Core, React 19, and PostgreSQL — following Clean Architecture / Onion Architecture principles.

**Backend:** .NET 10, ASP.NET Core Minimal APIs, EF Core 10, PostgreSQL 17, ASP.NET Core Identity, JWT Bearer auth, Finbuckle multi-tenancy, Serilog + Seq

**Frontend:** React 19, TypeScript 5.8, Vite 6, Tailwind v4, TanStack Query 5, React Router 7, Radix UI, i18next, Zustand

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (the `net10.0` target framework)
- [Node.js 22+](https://nodejs.org/) (frontend)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) with Docker Compose
- [dotnet-ef](https://www.nuget.org/packages/dotnet-ef) (installed via `dotnet tool restore`)

## Getting Started

```powershell
# Clone and restore
git clone <repo-url>
cd clean-architecture
dotnet tool restore
dotnet restore CleanArchitecture.slnx

# Build
dotnet build CleanArchitecture.slnx --configuration Release

# Run tests
dotnet test CleanArchitecture.slnx --configuration Release

# Start all services (API, Postgres, Seq, MailHog, UI)
docker compose up -d
```

### Services

| Service     | Port(s)              | Description                   |
|-------------|----------------------|-------------------------------|
| `web-api`   | 5000 (HTTP), 5001 (HTTPS) | ASP.NET Core backend     |
| `postgres`  | 5432                 | PostgreSQL 17 database        |
| `seq`       | 8081                 | Structured log viewer         |
| `mailhog`   | 1025 (SMTP), 8025 (UI) | Email testing              |
| `ui`        | 5173                 | React frontend (Vite dev)     |

### Local Frontend Development

```powershell
Set-Location src/ui
npm install
npm run dev          # http://localhost:5173, proxies /api to localhost:5000
npm run typecheck    # TypeScript type-check only
npm run test         # Vitest test runner
```

## Project Structure

```
clean-architecture/
├── src/
│   ├── SharedKernel/     # Zero-dependency foundation: Entity, Result, Error, domain events interfaces
│   ├── Domain/           # Entities, value objects, domain events, error definitions
│   ├── Application/      # CQRS (commands, queries, handlers), validators, decorators, abstractions
│   ├── Infrastructure/   # EF Core, Identity, JWT auth, multi-tenancy, email, external services
│   ├── Web.Api/          # Minimal API endpoints, Swagger, middleware, exception handling
│   └── ui/               # React 19 frontend (see src/ui/README.md)
├── tests/
│   └── ArchitectureTests/   # NetArchTest.Rules enforces layer dependency rules
├── docs/                    # Query pattern guides (paginated-list, tree-list, kanban-list)
├── CleanArchitecture.slnx   # Solution file (.slnx format)
├── Directory.Build.props    # Shared build properties (net10.0, warnings-as-errors)
├── Directory.Packages.props # Central package management
├── docker-compose.yml       # Docker Compose service definitions
└── .github/workflows/       # CI pipeline (build → test → publish)
```

## Architecture

### Layer Dependencies

Layer dependencies flow inward and are enforced at build time by `tests/ArchitectureTests`:

```
Web.Api → Infrastructure → Application → Domain → SharedKernel (no dependencies)
```

- **SharedKernel** has zero project dependencies. It provides the base abstractions used by every layer.
- **Domain** depends only on SharedKernel (plus `Microsoft.Extensions.Identity.Stores` for Identity entities).
- **Application** depends on Domain and SharedKernel. It defines use cases and orchestrates domain objects.
- **Infrastructure** depends on Application. It implements persistence, auth, email, and other external concerns.
- **Web.Api** depends on Infrastructure. It exposes HTTP endpoints and wires up the DI container.

Each layer's DI registration is an extension method called from `Program.cs`:

```csharp
builder.Services
    .AddApplication()
    .AddPresentation(builder.Environment)
    .AddInfrastructure(builder.Configuration, builder.Environment);
```

### SharedKernel

Foundational types with no external dependencies:

| Type                    | Purpose                                              |
|-------------------------|------------------------------------------------------|
| `Entity`                | Base class with `Id` and domain event infrastructure |
| `IDomainEventSource`    | Interface for entities that can raise domain events  |
| `Result` / `Result<T>`  | Discriminated union for success/failure outcomes     |
| `Error` / `ErrorType`   | Structured error representation                     |
| `IDomainEvent`          | Marker interface for domain events                   |
| `IDomainEventHandler<T>`| Handler contract for domain events                   |
| `IDateTimeProvider`     | Abstraction over `DateTime.UtcNow`                   |

### Domain

Contains the core business entities and rules. Does not depend on EF Core, ASP.NET, or any infrastructure concern.

Key entities:
- `User : IdentityUser<Guid>` — extends ASP.NET Core Identity. Implements `IDomainEventSource` manually.
- `Role : IdentityRole<Guid>` — Identity role with tenant-scoped permissions.
- `Tenant` — multi-tenant organization with subscription plan, status, and seat count.
- `Membership` — joins a `User` to a `Tenant` with a `Role`, forming the authorization backbone.
- `Invitation` — email-based invitation to join a tenant.
- `TodoItem` — example domain entity with priority, labels, status.
- `Payment` — Bakong KHQR payment records.

Domain entities raise domain events (e.g., `UserRegisteredDomainEvent`, `InvitationCreatedDomainEvent`) which are dispatched after `SaveChangesAsync` by the infrastructure layer.

### Application — CQRS

The application layer follows **CQRS** (Command Query Responsibility Segregation) without MediatR. Handlers are injected directly into Minimal API endpoints.

#### Commands

```csharp
// Marker interface
public interface ICommand<TResponse>;

// Command — a simple record/DTO
public sealed record LoginUserCommand(string Email, string Password) : ICommand<LoginResponse>;

// Handler — uses primary constructor for DI
internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        // ... domain logic ...
        return Result.Success(response);
    }
}
```

#### Queries

```csharp
public interface IQuery<TResponse>;

public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantResponse>;

internal sealed class GetTenantByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    public async Task<Result<TenantResponse>> Handle(GetTenantByIdQuery query, CancellationToken cancellationToken)
    {
        TenantResponse? tenant = await context.Tenants
            .Where(t => t.Id == query.TenantId)
            .Select(t => new TenantResponse(t.Id, t.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
            return Result.Failure<TenantResponse>(TenantErrors.NotFound(query.TenantId));

        return tenant;
    }
}
```

#### Decorator Pipeline

Handlers are registered via **Scrutor** assembly scanning and decorated in this order:

```
LoggingDecorator → ValidationDecorator → RealHandler    (commands only)
LoggingDecorator → RealHandler                           (queries only)
```

Validation uses **FluentValidation**. Validators are auto-discovered from the Application assembly.

### Infrastructure

#### Database (EF Core + PostgreSQL)

- `ApplicationDbContext` inherits from `IdentityDbContext<User, Role, Guid>`
- Uses **snake_case** naming convention via `EFCore.NamingConventions`
- `TenantSaveInterceptor` automatically sets `TenantId` on all new entities during `SaveChangesAsync` — anything with a `TenantId` property gets it populated from the current multi-tenant context
- Migrations live in `src/Infrastructure/Database/Migrations/`

#### Authentication (JWT)

- JWT tokens contain `sub`, `email`, `jti`, `tenant_ids`, `is_system_admin`, and `security_stamp` claims
- **Security stamp validation**: on every request, the `security_stamp` claim is compared against the stored value. Password changes invalidate all existing JWTs instantly.
- **Refresh tokens**: SHA256-hashed, rotated on use, theft detection (revokes all tokens if a reused token is detected), 7-day expiry
- **Two-factor authentication**: TOTP (authenticator app) via `/api/users/enable-2fa`, `/api/users/confirm-2fa`, `/api/users/login-2fa`

#### Authorization (Dynamic Policies)

Permission and feature policies are created on-the-fly — no static policy registration needed:

```csharp
// On an endpoint:
.HasPermission("users:write")       // checked against user's role permissions
.HasFeature("kanban-board")         // checked against tenant's subscription features
```

- `PermissionAuthorizationPolicyProvider` creates policies dynamically when a policy name is not found
- `PermissionAuthorizationHandler` queries the database for the user's permissions (via Membership → Role → RolePermissions) and grants access
- `SubscriptionFeatureAuthorizationHandler` queries the tenant's enabled subscription features
- System admins (`is_system_admin` claim) bypass all permission checks

#### Multi-Tenancy (Finbuckle)

Every request resolves a tenant:

1. `UseAuthentication()` — JWT is validated, claims extracted
2. `UseMultiTenant()` — Finbuckle resolves the tenant using `TenantMembershipStrategy`
3. `UseTenantEnforcement()` — blocks requests without a resolved tenant (except endpoints marked `.ExcludeFromMultiTenantResolution()`)
4. `UseAuthorization()` — permission/feature checks run last

- `AppTenantInfo` extends Finbuckle's `TenantInfo` with subscription metadata
- `TenantMembershipStrategy` reads `tenant_ids` from the JWT and matches against the `X-Tenant-Id` header
- `TenantStore` is an EF Core-backed `IMultiTenantStore<AppTenantInfo>`
- `UserContext.TenantId` provides the current tenant ID to application code via `IMultiTenantContextAccessor<AppTenantInfo>`

### Web.Api — Endpoints

Endpoints follow the `IEndpoint` pattern (not controller classes):

```csharp
internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            Request request,
            ICommandHandler<LoginUserCommand, LoginResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            Result<LoginResponse> result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Auth);
    }
}
```

Key patterns:
- Every endpoint is `internal sealed` implementing `IEndpoint`
- Handlers are injected directly into the Minimal API delegate — no MediatR
- `Result.Match(Results.Ok, CustomResults.Problem)` maps success/failure to HTTP responses (RFC 7807 problem details on failure)
- Use `.RequireAuthorization()`, `.HasPermission("...")`, or `.HasFeature("...")` for access control
- Use `.ExcludeFromMultiTenantResolution()` for endpoints that don't need a tenant context (login, register, admin)
- Endpoints are auto-discovered via assembly scanning in `EndpointExtensions.AddEndpoints()` and mapped under `/api/v{version:apiVersion}`

#### Result Pattern

All handlers return `Result` or `Result<T>`. Endpoints use the `Match` extension to produce HTTP responses:

```csharp
// For Result<T>:
result.Match(Results.Ok, CustomResults.Problem)

// For Result (no value):
result.Match(Results.NoContent, CustomResults.Problem)

// With custom success mapping:
result.Match(value => Results.Created($"/todos/{value}", value), CustomResults.Problem)
```

### Domain Events

Domain events provide a decoupled way to trigger side effects after a domain operation.

1. Domain entities implement `IDomainEventSource` and call `Raise(domainEvent)` to enqueue events
2. `ApplicationDbContext.SaveChangesAsync` extracts events from tracked entities, saves changes, then dispatches events
3. `DomainEventsDispatcher` resolves `IDomainEventHandler<T>` implementations from DI using cached reflection
4. Handlers live in the **Application** layer and perform side effects (send emails, etc.)

```
User entity → Raise(UserRegisteredDomainEvent) → SaveChangesAsync → DomainEventsDispatcher
    → UserRegisteredDomainEventHandler → IEmailService.SendAsync(...)
```

### Background Job Queue

A DB-backed outbox pattern with Redis notifications for asynchronous background processing.

1. `IBackgroundJob` (marker) + `IBackgroundJobHandler<T>` define jobs and their handlers
2. `IBackgroundJobQueue.EnqueueAsync<T>(T job)` persists jobs to the `background_jobs` table + publishes Redis notification
3. `BackgroundJobProcessor` (`BackgroundService`) subscribes to Redis for instant wake, polls DB every 10s as fallback
4. Failed jobs retry up to `MaxRetries` (default 3); resolved via cached reflection (same pattern as domain events)
5. Email is already wired: `IEmailService` → `QueuedEmailService` → `IBackgroundJobQueue.EnqueueAsync(SendEmailJob)`

See `docs/background-job-queue.md` for the full guide on adding new job types.

### Subscription Plans & Feature Gating

- **Domain**: `SubscriptionPlan`, `SubscriptionFeature`, `SubscriptionLimit` define per-tenant plan capabilities
- **Infrastructure**: `SubscriptionFeatureAuthorizationHandler` gates endpoints on subscription features
- Default plans, limits, and features defined in `Domain/SubscriptionFeatures/Default*.cs`
- Payment integration uses **Bakong KHQR** (`Kh.Gov.Nbc.BakongKHQR` NuGet package)

### Query Patterns

The project includes three reusable query result models documented in `docs/`:

| Pattern          | File                     | Use Case                                              |
|------------------|--------------------------|-------------------------------------------------------|
| Paginated List   | `docs/paginated-list.md` | Paged, sorted, filtered API responses                 |
| Tree List        | `docs/tree-list.md`      | Hierarchical data from flat database records           |
| Kanban List      | `docs/kanban-list.md`    | Grouped results into columns (by status, enum, etc.)   |

These are extension methods on `IQueryable<T>` in `src/Application/Extensions/PaginatedQueryExtensions.cs`.

## Adding a New Feature

Here is the recommended workflow for adding a new feature end-to-end. Example: "Archive TodoItem".

### 1. Domain Layer

Add behavior to the entity or create a domain event:

```csharp
// src/Domain/Todos/TodoItem.cs
public Result Archive()
{
    if (IsArchived)
        return Result.Failure(TodoItemErrors.AlreadyArchived(Id));

    IsArchived = true;
    Raise(new TodoItemArchivedDomainEvent(Id));
    return Result.Success();
}
```

Create error definitions if needed:

```csharp
// src/Domain/Todos/TodoItemErrors.cs
public static Error AlreadyArchived(Guid todoItemId) => Error.Conflict(
    "TodoItems.AlreadyArchived", $"The todo item with Id '{todoItemId}' is already archived.");
```

### 2. Application Layer

Create a command and handler:

```csharp
// src/Application/Todos/Archive/ArchiveTodoCommand.cs
public sealed record ArchiveTodoCommand(Guid TodoItemId) : ICommand;

// src/Application/Todos/Archive/ArchiveTodoCommandHandler.cs
internal sealed class ArchiveTodoCommandHandler(IApplicationDbContext context)
    : ICommandHandler<ArchiveTodoCommand>
{
    public async Task<Result> Handle(ArchiveTodoCommand command, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == command.TodoItemId, cancellationToken);

        if (todoItem is null)
            return Result.Failure(TodoItemErrors.NotFound(command.TodoItemId));

        return todoItem.Archive();
    }
}
```

Optionally add a FluentValidation validator:

```csharp
// src/Application/Todos/Archive/ArchiveTodoCommandValidator.cs
internal sealed class ArchiveTodoCommandValidator : AbstractValidator<ArchiveTodoCommand>
{
    public ArchiveTodoCommandValidator()
    {
        RuleFor(x => x.TodoItemId).NotEmpty();
    }
}
```

### 3. Web.Api Layer

Create an endpoint:

```csharp
// src/Web.Api/Endpoints/Todos/Archive.cs
internal sealed class Archive : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("todos/{todoItemId:guid}/archive", async (
            Guid todoItemId,
            ICommandHandler<ArchiveTodoCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ArchiveTodoCommand(todoItemId);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
```

If an endpoint needs permission or feature gating, add `.HasPermission(...)` or `.HasFeature(...)`. If it does not require a tenant context (auth, admin, system-level operations), add `.ExcludeFromMultiTenantResolution()`.

### 4. Frontend (optional)

Add an API client function in `src/ui/src/api/todos.ts`, a TanStack Query mutation hook, and wire it into the relevant page component. See `src/ui/README.md` for frontend conventions.

## EF Core Migrations

```powershell
# Install the EF tool (once)
dotnet tool restore

# Create a new migration
dotnet ef migrations add NameOfMigration

# Apply pending migrations
dotnet ef database update
```

- Migrations use **snake_case** naming convention
- In Development, migrations are auto-applied at startup via `ApplyMigrations()` in `MigrationExtensions.cs`
- The `TenantSaveInterceptor` automatically populates `TenantId` on new entities — ensure any new entity with tenant scope includes a `TenantId` property

## Code Conventions

These are enforced by `.editorconfig` and `Directory.Build.props`. Violations are **build errors**.

| Rule                                          | Example                                    |
|-----------------------------------------------|--------------------------------------------|
| Explicit typing (no `var`)                    | `List<string> items = [];` not `var items` |
| `var` only when type is apparent from RHS    | `var user = new User();` is allowed        |
| `is null` / `is not null`                    | Never `== null` / `!= null`               |
| File-scoped namespaces                        | `namespace Foo.Bar;` (no braces)           |
| `internal sealed` by default                  | Default visibility for classes             |
| Primary constructors for DI                   | `class Foo(IBar bar) { }`                  |
| Braces always required                        | Even for single-statement blocks           |
| Predefined types                              | `string` not `String`, `int` not `Int32`   |
| Warnings are errors                           | `TreatWarningsAsErrors=true`               |
| Code style is enforced at build               | `EnforceCodeStyleInBuild=true`             |

## Testing

Currently, only architecture tests exist (`tests/ArchitectureTests/Layers/LayerTests.cs`). They use **NetArchTest.Rules** to verify layer dependency rules.

```powershell
# Run all tests
dotnet test CleanArchitecture.slnx --configuration Release

# Run a specific test
dotnet test CleanArchitecture.slnx --filter "FullyQualifiedName~LayerTests"
```

The test project uses **xUnit** with **Shouldly** assertions. `InternalsVisibleTo` attributes are already declared for future unit/integration test projects.

## Docker Compose

```powershell
# Start all services
docker compose up -d

# View logs
docker compose logs -f web-api

# Rebuild after code changes
docker compose up -d --build

# Stop all services
docker compose down
```

The `docker-compose.override.yml` mounts user secrets and sets `ASPNETCORE_ENVIRONMENT=Development`.

## CI/CD

GitHub Actions workflow at `.github/workflows/build.yml` runs on every push to `main`:

```
dotnet restore → dotnet build (Release) → dotnet test (Release) → dotnet publish (Release)
```

Dependabot is configured for daily NuGet updates in `.github/dependabot.yml`.

## Useful Commands

| Task                           | Command                                                                 |
|--------------------------------|-------------------------------------------------------------------------|
| Restore                        | `dotnet restore CleanArchitecture.slnx`                                 |
| Build (Release)                | `dotnet build CleanArchitecture.slnx --configuration Release`           |
| Test                           | `dotnet test CleanArchitecture.slnx --configuration Release`            |
| Single test                    | `dotnet test CleanArchitecture.slnx --filter "FullyQualifiedName~Test"` |
| Docker Compose up              | `docker compose up -d`                                                  |
| Docker Compose down            | `docker compose down`                                                   |
| EF migration                   | `dotnet ef migrations add NameOfMigration`                              |
| EF update                      | `dotnet ef database update`                                             |
| Frontend dev                   | `npm run dev` (in `src/ui`)                                            |
| Frontend typecheck             | `npm run typecheck` (in `src/ui`)                                      |
| Frontend test                  | `npm run test` (in `src/ui`)                                           |

## Further Reading

- [AGENTS.md](./AGENTS.md) — terse reference for AI coding agents
- [src/ui/README.md](./src/ui/README.md) — frontend project guide
- [docs/paginated-list.md](./docs/paginated-list.md) — paginated query pattern
- [docs/tree-list.md](./docs/tree-list.md) — hierarchical query pattern
- [docs/kanban-list.md](./docs/kanban-list.md) — kanban query pattern
- [docs/background-job-queue.md](./docs/background-job-queue.md) — DB-backed background job queue with Redis
