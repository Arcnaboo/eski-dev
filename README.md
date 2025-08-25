# eski-dev

Monorepo for the legacy/full-stack sample used as a teaching reference.  
It contains a complete backend with background services, EF Core data layer, and admin utilities.

## Monorepo layout

```
Project1.sln
.vs/                 # IDE artifacts (ignored)
Gold.Api/            # ✅ Main backend application (Web API / workers)
Gold.Core/           # ✅ Primitive/domain entities & basic abstractions (no EF deps)
Gold.Domain/         # ✅ EF Core layer: DbContext, configurations, repositories
Gold.DB/             # Database assets: migrations, seed scripts, utilities
Gold.Admin/          # Admin/ops utilities (CLI or small tools)
Project1/            # Playground / sample host (optional) ─ remove if unused
```

### Responsibilities

- **Gold.Api**  
  Entry point. Hosts HTTP endpoints and background jobs. Wires DI, logging, configuration, and the EF Core `DbContext` from **Gold.Domain**.

- **Gold.Core**  
  Pure domain primitives (entities, value objects, enums, DTOs, exceptions, interfaces).  
  **No** references to EF Core or infrastructure. Safe to reuse in tests/tools.

- **Gold.Domain**  
  Persistence and data access with **EF Core** (DbContext, entity type configurations, repositories/specifications, unit of work if used). Maps **Gold.Core** entities to the database.

- **Gold.DB**  
  Database-specific assets (SQL scripts, migration helpers, seed data). You may keep EF migrations here or in **Gold.Domain**—choose one and stick with it.

- **Gold.Admin**  
  Operational/admin tools: data fixes, import/export, maintenance jobs, one-off scripts.

- **Project1**  
  Scratch host or sample app. If not needed, delete to reduce noise.

## Tech stack

- **.NET** (SDK version: _TBD_ – edit this once confirmed)
- **Entity Framework Core** for ORM
- Database: _TBD_ (SQL Server / PostgreSQL / SQLite).  
- Logging: built-in `ILogger<>` (Serilog/NLog if added)
- DI: built-in Microsoft.Extensions.DependencyInjection

## Getting started

### 1) Prereqs

- .NET SDK (_e.g., 8.0_).  
  Verify: `dotnet --version`
- A running database (_SQL Server/Postgres/SQLite_) and a connection string.

### 2) Configuration

In **Gold.Api** (and any other host), set up `appsettings.{Environment}.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Gold;User Id=...;Password=...;"
  }
}
```

You can also use environment variables:

```
ConnectionStrings__Default=<your-connection-string>
ASPNETCORE_ENVIRONMENT=Development
```

### 3) Restore & build

```bash
dotnet restore Project1.sln
dotnet build   Project1.sln -c Release
```

### 4) Database & migrations

```bash
# Add a migration
dotnet ef migrations add Initial --project Gold.Domain --startup-project Gold.Api

# Update DB
dotnet ef database update --project Gold.Domain --startup-project Gold.Api
```

### 5) Run the API

```bash
dotnet run --project Gold.Api
```

API should be available at `http://localhost:5xxx`.

## Maintainers

- **Arc** (repo owner)
