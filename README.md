# ServiceApp — Smart Service Marketplace API

A backend for a local-services marketplace (cleaning, plumbing, electrical, …) where
**clients** book **providers** for a given **service**. Built with **.NET 10** and
**Clean Architecture**.

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

## Roadmap (from the original plan)
AI provider matching · background notification jobs · ratings aggregation · SignalR chat ·
free-tier deployment.
