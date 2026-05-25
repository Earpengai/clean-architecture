# AGENTS.md

## Build & Test Commands

```powershell
# Restore, build, test (uses .slnx, not .sln)
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx --configuration Release
dotnet test CleanArchitecture.slnx --configuration Release

# Run a single test
dotnet test CleanArchitecture.slnx --filter "FullyQualifiedName~LayerTests"

# Docker Compose
docker compose up -d
# web-api on 5000/5001, postgres on 5432, seq on 8081, mailhog on 1025/8025, UI on 5173
docker compose down
```

## Architecture (Clean Architecture / Onion)

Layer dependencies (enforced by `tests/ArchitectureTests`):

```
Web.Api → Infrastructure → Application → Domain → SharedKernel (no deps)
```

- **SharedKernel**: `Entity`, `IDomainEventSource`, `Result`/`Error`, `IDomainEvent`, `IDateTimeProvider` — zero dependencies
- **Domain**: `User : IdentityUser<Guid>`, `Role : IdentityRole<Guid>`, `Tenant`, `Membership`, `Invitation`, `TodoItem`, `Payment`, `SubscriptionPlan`/`SubscriptionFeature`/`SubscriptionLimit`, domain events, error definitions — depends on SharedKernel + `Microsoft.Extensions.Identity.Stores`
- **Application**: CQRS (commands/queries/handlers), FluentValidation, decorators, interfaces (`IApplicationDbContext`, `ITokenProvider`, `IUserContext`, `IEmailService`) — depends on Domain + SharedKernel + `Microsoft.Extensions.Identity.Stores`
- **Infrastructure**: EF Core (`IdentityDbContext<User, Role, Guid>`), JWT auth, **Finbuckle multi-tenancy**, permission authorization, ASP.NET Core Identity stores, `DomainEventsDispatcher` — depends on Application + `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- **Web.Api**: Minimal API endpoints (`IEndpoint`), Swagger, Serilog, health checks, global exception handler — depends on Infrastructure
- **ArchitectureTests**: NetArchTest.Rules enforces the dependency rules above

## Multi-Tenancy

**Finbuckle.MultiTenant** — every request resolves a tenant from the JWT claim or route. Key pieces:

- `AppTenantInfo` (`src/Infrastructure/Multitenancy/`) — tenant identity model
- `TenantSaveInterceptor` — automatically sets `TenantId` on all new entities during `SaveChangesAsync`. Anything with a `TenantId` property gets it auto-populated from the current multi-tenant context.
- `TenantEnforcementMiddleware` — blocks requests without a resolved tenant (skips endpoints marked `IExcludeFromMultiTenantResolutionMetadata`)
- `TenantStore` — EF Core-backed `IMultiTenantStore<AppTenantInfo>`
- `UserContext.TenantId` — application code reads tenant context from `IMultiTenantContextAccessor<AppTenantInfo>`
- Domain entities with tenant scope: `Tenant`, `Membership`, `Invitation`, `Role`, `TodoItem`, `Payment`

## Subscription Plans & Feature Gating

- Domain: `SubscriptionPlan`, `SubscriptionFeature`, `SubscriptionLimit` — per-tenant plan definitions
- `SubscriptionFeatureAuthorizationHandler` (`src/Infrastructure/Authorization/`) — custom authorization handler that gates endpoints on subscription features
- Default plans/limits/features defined in `Domain/SubscriptionFeatures/Default*.cs`
- Payment integration uses **Bakong KHQR** (`Kh.Gov.Nbc.BakongKHQR` NuGet package)

## Frontend (src/ui)

React 19 + Vite 6 + TypeScript + Tailwind v4 frontend. Runs in Docker as the `ui` service, or standalone:

```powershell
Set-Location -LiteralPath "src/ui"
npm install
npm run dev          # dev server at http://localhost:5173, proxies /api
npm run build        # type-check + production build
npm run typecheck    # TypeScript type-check only
npm run test         # Vitest test runner
npm run preview      # preview production build
```

Key packages: React Router 7, TanStack Query 5, Radix UI, Lucide React, i18next.
Node 22+ required. Runs in Docker compose as `ui` service on port 5173.

## Command Order

```
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx --no-restore
dotnet test CleanArchitecture.slnx --no-build
```

CI (`github/workflows/build.yml`) runs the exact same sequence on push to `main`.

## EF Core Migrations

```powershell
dotnet tool restore                              # installs dotnet-ef (v10.0.7)
dotnet ef migrations add NameOfMigration         # create a new migration
dotnet ef database update                        # apply pending migrations
```

Migrations are auto-applied at startup in Development via `ApplyMigrations()` (`src/Web.Api/Extensions/MigrationExtensions.cs`). Note: currently uses `EnsureCreated()` not `Migrate()` in dev — `Migrate()` is commented out for production use.

## Key Conventions

- **Explicit typing** — avoid `var`; only use it when the type is apparent from the RHS (enforced by `.editorconfig`)
- **`is null` / `is not null`** — never `== null` / `!= null`
- **Primary constructors** for DI in services, use cases, etc.
- **Make types `internal sealed`** by default
- **Prefer `Guid`** for entity identifiers
- **File-scoped namespaces** (enforced)
- **Warnings are errors** — `TreatWarningsAsErrors=true`, `CodeAnalysisTreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true` in `Directory.Build.props`. Code style violations are build errors. No warning survives the build.
- **Central package management** — all NuGet versions in `Directory.Packages.props`; `.csproj` files omit `Version` attributes
- **EF Core uses snake_case** naming convention (`EFCore.NamingConventions` package)
- **SonarAnalyzer.CSharp** runs on all projects; many rules suppressed in `.editorconfig`
- **.cursorrules file is stale** — it targets .NET 8 and controller endpoints, but the project is .NET 10 with `IEndpoint` minimal API pattern. Do not trust `.cursorrules`.

## Framework & Toolchain

- **.NET 10** (`net10.0`) with `AnalysisLevel=latest`, `AnalysisMode=All`
- **PostgreSQL 17** via docker compose; database `clean-architecture`, user/pass `postgres/postgres`
- **Seq** for structured log viewing at `http://localhost:8081`
- **MailHog** for email testing at `http://localhost:8025` — SMTP on port 1025, no auth required
- **Serilog** with Seq sink; `RequestContextLoggingMiddleware` pushes correlation IDs to log context
- **JWT auth** with config from `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience` — use user secrets or env vars (secrets stored in `UserSecretsId: 7af30323-108a-4923-bd46-e13b7648968c`)
- **ASP.NET Core Identity** for user/role management: `AddIdentityCore<User>().AddRoles<Role>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()`
  - Handlers inject `UserManager<User>` for password hashing, lockout, email confirmation, password reset
  - Password rules: 8+ chars, require uppercase/lowercase/digit/non-alphanumeric
  - Lockout: 5 failed attempts → 15-minute lockout
  - Built-in token providers for email confirmation and password reset (replaces custom token fields on User entity)
- **Security stamp validation** — embedded in JWT `security_stamp` claim, validated on every request via `OnTokenValidated`. Password changes invalidate all existing JWTs.
- **Refresh tokens** — SHA256-hashed, rotation on use, reuse detection (revokes all tokens if token theft detected), 7-day expiry. `POST /api/users/refresh`
- **Two-Factor Authentication** — TOTP (authenticator app) via `POST /api/users/enable-2fa`, `POST /api/users/confirm-2fa`, `POST /api/users/disable-2fa`. Login returns `RequiresTwoFactor` when 2FA enabled; `POST /api/users/login-2fa` to verify.
- **Admin lock/unlock** — `POST /api/admin/users/{userId}/lock` and `POST /api/admin/users/{userId}/unlock` (system admin only)
- **Swagger/OpenAPI** at `/swagger` (dev only)

## CQRS Pattern

- Commands: `ICommand`, `ICommand<TResponse>` with handlers returning `Result` / `Result<T>`
- Queries: `IQuery<TResponse>` with handlers returning `Result<T>`
- Handlers are decorated at registration by **Scrutor**: `ValidationDecorator` (commands only) then `LoggingDecorator` (both)

## Domain Events

- Entities implement `IDomainEventSource` interface, call `Raise(domainEvent)` to enqueue events
- `Entity` base class provides the `IDomainEventSource` implementation for non-Identity entities (TodoItem, Membership, Tenant, Invitation, Payment)
- `User` and `Role` implement `IDomainEventSource` manually since they inherit from IdentityUser/IdentityRole
- `ApplicationDbContext.SaveChangesAsync` dispatches queued events via `DomainEventsDispatcher`
- `DomainEventsDispatcher` resolves `IDomainEventHandler<T>` implementations from the DI container using cached reflection
- Domain event handlers live in the **Application** layer, not Domain

## Background Job Queue

- **DB-backed outbox** — `BackgroundJob` entity persisted in `background_jobs` table, processed by `BackgroundJobProcessor` (`BackgroundService`)
- **Redis notifications** — `DatabaseBackgroundJobQueue` publishes on `"jobs:notifications"` after DB insert; processor subscribes for instant wake, falls back to 10s polling
- **Retry on failure** — up to `MaxRetries` (default 3); status cycles `Pending → Processing → (Pending | Failed)`
- **Email is queued** — `IEmailService → QueuedEmailService → IBackgroundJobQueue.EnqueueAsync(SendEmailJob)`; domain event handlers unchanged
- **Add new jobs** — implement `IBackgroundJob` (marker record) + `IBackgroundJobHandler<T>`, register in DI; see `docs/background-job-queue.md`

## Web.Api Endpoint Pattern

Uses `IEndpoint` interface (not controller classes) with `MapEndpoint(IEndpointRouteBuilder)` — registered via assembly scanning in DI. Return values use `Result` with `Match()` extension methods and `CustomResults.Problem()` for RFC 7807 problem details.

## Testing

- Only architecture tests exist (`tests/ArchitectureTests`). No unit/functional/integration tests yet (though `InternalsVisibleTo("Application.UnitTests")` is declared).
- `Program.cs` is declared `public partial class Program` for integration test accessibility.
