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

- **SharedKernel**: `Entity`, `Result`/`Error`, `IDomainEvent`, `IDateTimeProvider` — zero dependencies
- **Domain**: `TodoItem`, `User`, domain events, error definitions — depends only on SharedKernel
- **Application**: CQRS (commands/queries/handlers), FluentValidation, decorators, interfaces (`IApplicationDbContext`, `IPasswordHasher`, etc.) — depends on Domain + SharedKernel
- **Infrastructure**: EF Core + PostgreSQL, JWT auth, permission authorization, `DomainEventsDispatcher` — depends on Application
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
- **Serilog** with Seq sink; `RequestContextLoggingMiddleware` pushes correlation IDs to log context
- **JWT auth** with config from `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience` — use user secrets or env vars (secrets stored in `UserSecretsId: 7af30323-108a-4923-bd46-e13b7648968c`)
- **Swagger/OpenAPI** at `/swagger` (dev only)
- **EF Core migrations** auto-applied in Development via `ApplyMigrations()` at startup

## CQRS Pattern

- Commands: `ICommand`, `ICommand<TResponse>` with handlers returning `Result` / `Result<T>`
- Queries: `IQuery<TResponse>` with handlers returning `Result<T>`
- Handlers are decorated at registration by **Scrutor**: `ValidationDecorator` (commands only) then `LoggingDecorator` (both)

## Domain Events

- Entities inherit `Entity` base class, call `Raise(domainEvent)` to enqueue events
- `ApplicationDbContext.SaveChangesAsync` dispatches queued events via `DomainEventsDispatcher`
- `DomainEventsDispatcher` resolves `IDomainEventHandler<T>` implementations from the DI container using cached reflection
- Domain event handlers live in the **Application** layer, not Domain

## Web.Api Endpoint Pattern

Uses `IEndpoint` interface (not controller classes) with `MapEndpoint(IEndpointRouteBuilder)` — registered via assembly scanning in DI. Return values use `Result` with `Match()` extension methods and `CustomResults.Problem()` for RFC 7807 problem details.

## Testing

- Only architecture tests exist (`tests/ArchitectureTests`). No unit/functional/integration tests yet (though `InternalsVisibleTo("Application.UnitTests")` is declared).
- `Program.cs` is declared `public partial class Program` for integration test accessibility.
