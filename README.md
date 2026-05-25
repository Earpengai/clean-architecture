# Clean Architecture Template

A production-ready, multi-tenant SaaS application template built with .NET 10, ASP.NET Core, React 19, and PostgreSQL — following Clean Architecture / Onion Architecture principles.

## Features

| Category              | Details                                                                 |
|-----------------------|-------------------------------------------------------------------------|
| Architecture          | Clean Architecture with enforced layer dependencies (SharedKernel → Domain → Application → Infrastructure → Web.Api) |
| CQRS                  | Command/query separation with Scrutor decorator pipeline (validation → logging) |
| Multi-Tenancy         | Finbuckle.MultiTenant with JWT-claim-based tenant resolution, `X-Tenant-Id` header support, and auto-populated `TenantId` on new entities |
| Authentication        | ASP.NET Core Identity, JWT Bearer with security stamp validation, refresh token rotation with theft detection, TOTP two-factor authentication |
| Authorization         | Dynamic permission-based policies (created on-the-fly, no static registration) with system admin bypass |
| Subscription Gating   | Per-tenant subscription plans, feature flags, and usage limits with authorization handler integration |
| Database              | EF Core 10 + PostgreSQL 17 with snake_case naming, `TenantSaveInterceptor`, and auto-applied migrations |
| Domain Events         | Entity-level event sourcing with cached-reflection dispatcher and application-layer handlers |
| Payments              | Bakong KHQR payment integration (Cambodia's national QR payment standard) |
| API                   | Minimal API endpoints via `IEndpoint` pattern, API versioning, RFC 7807 problem details, Swagger/OpenAPI |
| Logging               | Serilog with structured logging and Seq sink at `http://localhost:8081` |
| Email Testing         | MailHog SMTP server at `http://localhost:8025` |
| Frontend              | React 19, TypeScript 5.8, Vite 6, Tailwind v4, TanStack Query 5, React Router 7, Radix UI, i18next |
| Testing               | Architecture tests via NetArchTest.Rules enforcing layer dependency rules |

## Getting Started

```powershell
git clone <repo-url>
cd clean-architecture
dotnet tool restore
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx --configuration Release
docker compose up -d
```

### Services

| Service     | Port(s)                  | Description               |
|-------------|--------------------------|---------------------------|
| Web API     | `http://localhost:5000`  | ASP.NET Core backend      |
| Postgres    | `localhost:5432`         | PostgreSQL 17             |
| Seq         | `http://localhost:8081`  | Structured log viewer     |
| MailHog     | `http://localhost:8025`  | Email testing UI          |
| UI          | `http://localhost:5173`  | React frontend            |

## Documentation

| Document            | Description                                     |
|---------------------|-------------------------------------------------|
| [DEVELOPER.md](./DEVELOPER.md) | Architecture deep dive and contribution guide |
| [AGENTS.md](./AGENTS.md)       | AI coding agent reference                      |
| [src/ui/README.md](./src/ui/README.md) | Frontend project guide                |
| [docs/paginated-list.md](./docs/paginated-list.md) | Paginated query pattern           |
| [docs/tree-list.md](./docs/tree-list.md)       | Hierarchical query pattern          |
| [docs/kanban-list.md](./docs/kanban-list.md)   | Kanban query pattern               |
