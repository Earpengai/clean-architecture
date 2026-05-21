# AGENTS.md

## Build & Test Commands

```powershell
# Restore, build, test (uses .slnx, not .sln)
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx --configuration Release
dotnet test CleanArchitecture.slnx --configuration Release

# Run a single test
dotnet test CleanArchitecture.slnx --filter "FullyQualifiedName~LayerTests"

# Docker Compose (web-api, postgres, seq)
docker compose up -d    # web-api on 5000/5001, postgres on 5432, seq on 8081
docker compose down
```

## Architecture (Clean Architecture / Onion)

Layer dependencies (enforced by `tests/ArchitectureTests`):

```
Web.Api → Infrastructure → Application → Domain → SharedKernel (no deps)
```

- **SharedKernel**: `Entity`, `IDomainEventSource`, `Result`/`Error`, `IDomainEvent`, `IDateTimeProvider` — zero dependencies
- **Domain**: `User : IdentityUser<Guid>`, `Role : IdentityRole<Guid>`, `TodoItem`, domain events, error definitions — depends on SharedKernel + `Microsoft.Extensions.Identity.Stores`
- **Application**: CQRS (commands/queries/handlers), FluentValidation, decorators, interfaces (`IApplicationDbContext`, `ITokenProvider`, `IUserContext`) — depends on Domain + SharedKernel + `Microsoft.Extensions.Identity.Stores`
- **Infrastructure**: EF Core (`IdentityDbContext<User, Role, Guid>`), JWT auth, permission authorization, ASP.NET Core Identity stores, `DomainEventsDispatcher` — depends on Application + `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- **Web.Api**: Minimal API endpoints (`IEndpoint`), Swagger, Serilog, health checks — depends on Infrastructure
- **ArchitectureTests**: NetArchTest.Rules enforces the dependency rules above

## Command Order

```
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx --no-restore
dotnet test CleanArchitecture.slnx --no-build
```

## Key Conventions

- **Explicit typing** — avoid `var`; only use it when the type is apparent from the RHS (enforced by `.editorconfig`)
- **`is null` / `is not null`** — never `== null` / `!= null`
- **Primary constructors** for DI in services, use cases, etc.
- **Make types `internal sealed`** by default unless otherwise specified
- **Prefer `Guid`** for entity identifiers
- **File-scoped namespaces** (enforced)
- **Warnings are errors** — `TreatWarningsAsErrors` and `CodeAnalysisTreatWarningsAsErrors` both `true` in `Directory.Build.props`. No warning will survive the build.
- **Central package management** — all NuGet versions live in `Directory.Packages.props`; `.csproj` files omit `Version` attributes
- **EF Core uses snake_case** naming convention (`EFCore.NamingConventions` package)
- **SonarAnalyzer.CSharp** runs on all projects; many rules suppressed in `.editorconfig`

## Framework & Toolchain

- **.NET 10** (`net10.0`) — `Directory.Build.props` has commented-out net8.0/net9.0 alternatives
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
- **EF Core migrations** auto-applied in Development via `ApplyMigrations()` at startup

## CQRS Pattern

- Commands: `ICommand`, `ICommand<TResponse>` with handlers returning `Result` / `Result<T>`
- Queries: `IQuery<TResponse>` with handlers returning `Result<T>`
- Handlers are decorated at registration by **Scrutor**: `ValidationDecorator` (commands only) then `LoggingDecorator` (both)

## Domain Events

- Entities implement `IDomainEventSource` interface, call `Raise(domainEvent)` to enqueue events
- `Entity` base class provides the `IDomainEventSource` implementation for non-Identity entities (TodoItem, Membership, Tenant, Invitation)
- `User` and `Role` implement `IDomainEventSource` manually since they inherit from IdentityUser/IdentityRole
- `ApplicationDbContext.SaveChangesAsync` dispatches queued events via `DomainEventsDispatcher`
- `DomainEventsDispatcher` resolves `IDomainEventHandler<T>` implementations from the DI container using cached reflection
- Domain event handlers live in the **Application** layer, not Domain

## Web.Api Endpoint Pattern

Uses `IEndpoint` interface (not controller classes) with `MapEndpoint(IEndpointRouteBuilder)` — registered via assembly scanning in DI. Return values use `Result` with `Match()` extension methods and `CustomResults.Problem()` for RFC 7807 problem details.

## Testing

- Only architecture tests exist (`tests/ArchitectureTests`). No unit/functional/integration tests yet (though `InternalsVisibleTo("Application.UnitTests")` is declared).
- `Program.cs` is declared `public partial class Program` for integration test accessibility.
