# ServiceApp — Smart Service Marketplace API

[![CI-CD](https://github.com/PaolaQuintanilla/marketplace-service-app/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/PaolaQuintanilla/marketplace-service-app/actions/workflows/ci-cd.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED)
![Azure](https://img.shields.io/badge/Azure-App%20Service-0078D4)

A backend for a local-services marketplace (cleaning, plumbing, electrical, …) where
**clients** book **providers** for a given **service**. Built with **.NET 10** and
**Clean Architecture**, containerized with **Docker**, and continuously deployed to **Azure**
via **GitHub Actions**.

## 🚀 Live demo

- **Swagger UI:** https://serviceapp-api-pq1.azurewebsites.net/swagger
- **Health check:** https://serviceapp-api-pq1.azurewebsites.net/health

> ⏳ Hosted on Azure's free tier — the **first request may take ~20s** while the app wakes from
> sleep (cold start). Log in with the seeded admin below to try the protected endpoints.

## Architecture

```
ServiceApp.Domain          Entities, enums, repository/UoW interfaces (no dependencies)
ServiceApp.Application      DTOs, business services, AutoMapper, validation, exceptions
ServiceApp.Infrastructure   EF Core (SQL Server), repositories, Unit of Work, JWT + BCrypt
ServiceApp.API              ASP.NET Core controllers, JWT auth, Swagger, error middleware
```

Dependency flow: `API → Application + Infrastructure → Domain`. Each layer exposes an
`AddApplication()` / `AddInfrastructure(config)` DI extension method.

### Patterns used
Clean Architecture · Repository + Unit of Work · Service layer · Dependency Injection ·
DTOs with AutoMapper · LINQ · JWT auth with role-based authorization · RFC 7807 ProblemDetails.

## Getting started

### Option A — Docker (no local SQL Server needed)

Requires **Docker Desktop**. Brings up the API **and** SQL Server with one command:

```bash
docker compose up --build
```
Then open `http://localhost:8080/swagger` (health at `http://localhost:8080/health`).

### Option B — local .NET SDK

Requires the **.NET 10 SDK** and **SQL Server LocalDB** (default) or any SQL Server.

```bash
# from the solution root
dotnet run --project ServiceApp.API --launch-profile http
```

On startup the app **applies migrations** and **seeds** baseline data, then serves Swagger at
`http://localhost:5260/swagger`.

### Seeded accounts & data
| Item     | Value                                         |
|----------|-----------------------------------------------|
| Admin    | `admin@serviceapp.local` / `Admin123!`        |
| Services | Plumbing, Cleaning, Electrical, Gardening     |

## Configuration

`ServiceApp.API/appsettings.json`:
- `ConnectionStrings:Default` — SQL Server connection string.
- `Jwt:SecretKey` — **change for production** (min 32 chars). Prefer user-secrets / env vars.

```bash
# example: override the JWT secret without editing the file
dotnet user-secrets set "Jwt:SecretKey" "<a-long-random-secret>" --project ServiceApp.API
```

## API surface

| Method | Route                          | Auth            | Purpose                          |
|--------|--------------------------------|-----------------|----------------------------------|
| POST   | `/api/auth/register`           | anon            | Register (Client or Provider)    |
| POST   | `/api/auth/login`              | anon            | Login, returns JWT               |
| GET    | `/api/services`                | anon            | List service categories          |
| POST   | `/api/services`                | **Admin**       | Create a service                 |
| GET    | `/api/providers?serviceId=`    | anon            | Search providers (by rating)     |
| POST   | `/api/providers`               | auth            | Create own provider profile      |
| POST   | `/api/bookings`                | auth (client)   | Book a provider                  |
| GET    | `/api/bookings/mine`           | auth            | My bookings as a client          |
| GET    | `/api/bookings/assigned`       | auth (provider) | Bookings assigned to me          |
| PATCH  | `/api/bookings/{id}/status`    | auth            | Confirm/complete (provider) or cancel |

## Database / migrations

```bash
dotnet ef migrations add <Name> -p ServiceApp.Infrastructure -s ServiceApp.API -o Persistence/Migrations
dotnet ef database update      -p ServiceApp.Infrastructure -s ServiceApp.API
```

## Testing

A dedicated **xUnit** test project (`ServiceApp.Tests`) covers the core logic and the HTTP API:

- **Unit tests** (with **Moq**) for `AuthService`, `BookingService`, and password hashing.
- **Integration / API tests** that boot the real API in-process with `WebApplicationFactory`
  over an EF Core **in-memory** database — exercising routing, JWT auth, middleware, and the
  full booking lifecycle end to end.

```bash
dotnet test
```

## Deployment & DevOps

- **Docker** — multi-stage `Dockerfile` (SDK build → slim runtime image, non-root, port 8080)
  and a `docker-compose.yml` for the full local stack (API + SQL Server).
- **CI/CD** — GitHub Actions (`.github/workflows/ci-cd.yml`): build + test on every push/PR to
  `main`, then build the image, push it to **GHCR**, and deploy to **Azure App Service** on `main`.
- **Config & secrets** — injected as environment variables (the .NET `__` convention); no
  secrets are committed to the repo.

See [`DEPLOYMENT.md`](DEPLOYMENT.md) for the full Azure + GitHub Actions runbook.

## Roadmap (from the original plan)
AI provider matching · background notification jobs · ratings aggregation · SignalR chat ·
free-tier deployment.
