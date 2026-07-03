# Patsanstha — Cooperative Credit Society Management System

Modular monolith backend (.NET 8) + Angular microfrontend shell for managing members, deposits, loans, collections, recovery, accounting, and reporting for a cooperative credit society (NBFC).

---

## Prerequisites (local development)

| Requirement | Version | Required? | Notes |
|-------------|---------|-----------|-------|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | **8.0+** | Yes | Backend build & run |
| [Node.js](https://nodejs.org/) | **20+** | Yes (for UI) | Frontend (Nx + Angular 19) |
| [PostgreSQL](https://www.postgresql.org/) | **16+** | Yes | Single database: `patsanstha` |
| [Docker Desktop](https://www.docker.com/) | Latest | Optional | Can run Postgres + Redis via `docker-compose.yml` |
| [Redis](https://redis.io/) | 7+ | Optional | Dev falls back to in-memory cache if Redis is not configured |

### Minimum setup (backend only)

1. Install **.NET 8 SDK**
2. Install **PostgreSQL** and create database `patsanstha`
3. Update connection strings in `src/Host/Patsanstha.Api/appsettings.json` if your Postgres user/password differ

Example connection string (all modules share one database):

```
Host=localhost;Port=5432;Database=patsanstha;Username=postgres;Password=<your-password>
```

### Full stack setup (backend + frontend)

Add **Node.js 20+**, then from `frontend/`:

```bash
npm install
```

---

## Repository layout

```
NBFC/
├── Patsanstha.sln              # Main .NET solution
├── docker-compose.yml          # Optional: Postgres 16 + Redis 7
├── README.md                   # This file
├── Design-Screens/             # UI reference (HTML mockups + design docs)
│
├── src/
│   ├── Host/
│   │   ├── Patsanstha.Api/     # Modular monolith API (port 5080)
│   │   └── Patsanstha.Gateway/ # YARP reverse proxy (port 5000)
│   │
│   ├── Shared/
│   │   ├── BuildingBlocks/     # Domain, Application, Infrastructure (shared)
│   │   └── Contracts/          # Cross-module integration event contracts
│   │
│   └── Modules/                # Bounded contexts (Clean Architecture each)
│       ├── Identity/
│       ├── Members/
│       ├── Deposits/
│       ├── Loans/
│       ├── Recovery/
│       ├── Collections/
│       ├── Accounting/
│       └── Reporting/
│
└── frontend/                   # Nx monorepo (Angular microfrontends)
    ├── apps/
    │   ├── shell/              # Host app (port 4200)
    │   ├── mfe-member/
    │   ├── mfe-deposit/
    │   ├── mfe-loan/
    │   ├── mfe-collection/
    │   ├── mfe-recovery/
    │   ├── mfe-accounting/
    │   ├── mfe-reports/
    │   └── mfe-admin/
    └── libs/
        ├── auth/               # JWT, guards, interceptors
        ├── ui-kit/             # Shared UI components
        ├── design-tokens/
        ├── core-utils/
        └── data-access/        # Per-module API clients
```

Module conventions are documented in `src/Modules/README.md`.

---

## How the project works

### Architecture overview

```
Browser (Angular shell :4200)
        │
        ▼
YARP Gateway (:5000)  ──optional in dev──►  Patsanstha.Api (:5080)
        │                                           │
        └───────────────────────────────────────────┘
                                                    │
                    ┌───────────────────────────────┼───────────────────────────────┐
                    ▼                               ▼                               ▼
              Identity module                  Members module                   Loans module
              (JWT, roles)                     (CQRS + domain)                  (CQRS + domain)
                    │                               │                               │
                    └───────────────────────────────┴───────────────────────────────┘
                                                    │
                                            PostgreSQL (patsanstha)
                                            one schema per module
```

### Backend: modular monolith

All modules run inside **one ASP.NET Core host** (`Patsanstha.Api`). Each module follows **Clean Architecture**:

```
Modules/{ModuleName}/
  {ModuleName}.Domain/          Entities, aggregates, domain events
  {ModuleName}.Application/     CQRS commands/queries (MediatR)
  {ModuleName}.Infrastructure/  EF Core DbContext, repositories, services
  {ModuleName}.Api/             Minimal API endpoint mappings
```

**Registered modules:**

| Module | PostgreSQL schema | Purpose |
|--------|-------------------|---------|
| Identity | `identity` | Auth, JWT, roles, permissions |
| Members | `members` | Member onboarding & profiles |
| Deposits | `deposits` | Deposit accounts |
| Loans | `loans` | Loan applications & accounts |
| Recovery | `recovery` | Recovery cases |
| Collections | `collections` | EMI / payment collection |
| Accounting | `accounting` | Ledger & trial balance |
| Reporting | `reporting` | Reports & aggregations |

**Shared infrastructure** (`BuildingBlocks`):

| Component | Location |
|-----------|----------|
| `Result<T>` / `Error`, Entity / AggregateRoot / Money | `BuildingBlocks.Domain` |
| CQRS markers (`ICommand`, `IQuery`), MediatR behaviors | `BuildingBlocks.Application` |
| Audit interceptor, transactional outbox, `ModuleDbContextBase` | `BuildingBlocks.Infrastructure` |
| RFC 7807 error mapping, idempotency filter, Polly resilience | `BuildingBlocks.Infrastructure` |
| OpenTelemetry tracing, Serilog JSON logging | `BuildingBlocks.Infrastructure` / `Program.cs` |
| Hangfire background jobs (Postgres storage) | `BuildingBlocks.Infrastructure/BackgroundJobs` |
| Cross-module integration event contracts | `Contracts` |

**Module communication rules:**

- Modules talk to each other only via **integration events** in `Shared/Contracts`
- Events are published through the **transactional outbox**
- **No cross-schema foreign keys** — each module owns its schema

### Database

- **One database:** `patsanstha`
- **One schema per module** (see table above)
- Hangfire job tables also live in the same database
- In **Development**, migrations run automatically on API startup
- Default admin user is seeded in Development

### Frontend: microfrontends

- **Nx** workspace with **Angular 19** and **Native Federation**
- **shell** (port 4200) loads remote MFEs on demand
- Shared **auth**, **ui-kit**, and **data-access** libraries
- Dev CORS allows `http://localhost:4200` → API on `:5080`

### Gateway (optional locally)

`Patsanstha.Gateway` uses **YARP** to proxy all traffic from `:5000` → API `:5080`.  
In local dev you can call the API directly on `:5080` or use the gateway on `:5000`.

Redis is optional in Development (`appsettings.Development.json` clears `ConnectionStrings:Redis` and falls back to in-memory distributed cache). Production should set Redis for idempotency and session validation.

Recurring Hangfire job schedules are configured in the `BackgroundJobs` section of `appsettings.json` (EMI generation, interest accrual, ageing, notifications, report pre-aggregation).

---

## Local setup

### Option A — Docker for Postgres + Redis

From the `NBFC/` folder:

```bash
docker compose up -d
```

This starts:

- Postgres on `localhost:5432` (database: `patsanstha`)
- Redis on `localhost:6379`

### Option B — Local PostgreSQL (no Docker)

1. Install and start PostgreSQL
2. Create the database:

```sql
CREATE DATABASE patsanstha;
```

3. Set passwords in `src/Host/Patsanstha.Api/appsettings.json` to match your Postgres user

Redis is **not required** in Development — `appsettings.Development.json` clears the Redis connection and uses in-memory cache.

---

## Docker — build all images

Requires [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Compose).

### Images included

| Image | Service | Port |
|-------|---------|------|
| `patsanstha/api` | Backend API | 5080 |
| `patsanstha/gateway` | YARP gateway | 5000 |
| `patsanstha/shell` | Angular shell | 4200 |
| `patsanstha/mfe-member` | Member MFE | 4201 |
| `patsanstha/mfe-deposit` | Deposit MFE | 4202 |
| `patsanstha/mfe-loan` | Loan MFE | 4203 |
| `patsanstha/mfe-collection` | Collection MFE | 4204 |
| `patsanstha/mfe-recovery` | Recovery MFE | 4205 |
| `patsanstha/mfe-accounting` | Accounting MFE | 4206 |
| `patsanstha/mfe-reports` | Reports MFE | 4207 |
| `patsanstha/mfe-admin` | Admin MFE | 4208 |
| `postgres:16-alpine` | Database | 5432 |
| `redis:7-alpine` | Cache | 6379 |

### Build everything (from `NBFC/`)

```bash
# Build all 11 custom images (api, gateway, 9 frontend apps)
docker compose build

# Build in parallel (faster)
docker compose build --parallel
```

### Build subsets

```bash
# Infrastructure only
docker compose build postgres redis

# Backend only
docker compose build api gateway

# Frontend only (from NBFC/)
docker compose build shell mfe-member mfe-deposit mfe-loan mfe-collection mfe-recovery mfe-accounting mfe-reports mfe-admin

# Frontend only (from frontend/)
cd frontend
npm run docker:build
```

### Run full stack

```bash
# Local quick start (uses defaults in docker-compose if no .env)
docker compose up -d
```

For **VPS / production**, copy `.env.example` → `.env` first (see [VPS deployment](#vps-deployment) below).

Then open:

- Shell: http://localhost:4200
- API / Swagger: http://localhost:5080/swagger (Development only)
- Gateway: http://localhost:5000

Default admin (when using dev defaults): `admin@patsanstha.local` / `ChangeMe@123`

Frontend apps call the API at `http://localhost:5080` (hardcoded in app config). MFE remotes load from `localhost:4201`–`4208`.

On startup, the API applies migrations and seeds identity data when `Startup:AutoMigrate` / `Startup:SeedIdentity` are enabled (default `true` in Docker via `.env`).

---

## VPS deployment

Deploy the full stack on a Linux VPS with Docker Compose.

### 1. Clone and configure secrets

```bash
git clone <your-repo-url>
cd NBFC
cp .env.example .env
nano .env   # set POSTGRES_PASSWORD, JWT__SigningKey, PiiEncryption__Key, admin password
```

**Required variables in `.env`:**

| Variable | Purpose |
|----------|---------|
| `POSTGRES_PASSWORD` | Database password |
| `JWT__SigningKey` | JWT signing secret (min 32 chars) |
| `PiiEncryption__Key` | Aadhaar/PAN encryption key (min 32 chars) |
| `IdentitySeed__AdminPassword` | First admin login password |

Never commit `.env` — it is listed in `.gitignore`.

### 2. Build and start

```bash
docker compose build
docker compose up -d
```

### 3. Verify

```bash
curl http://localhost:5080/health/live
curl http://localhost:5000/health/live
```

### 4. Put nginx in front (recommended)

Expose only **HTTPS (443)** publicly. Proxy to Gateway (`:5000`) for API and to Shell (`:4200`) for the UI:

```nginx
# Example — adjust server_name and SSL paths
server {
    listen 443 ssl;
    server_name yourdomain.com;

    location /api/ {
        proxy_pass http://127.0.0.1:5000;
    }

    location / {
        proxy_pass http://127.0.0.1:4200;
    }
}
```

Postgres (`5432`) and Redis (`6379`) bind to `127.0.0.1` by default — not exposed to the internet.

### 5. Startup flags

| `.env` variable | Default | Purpose |
|-----------------|---------|---------|
| `STARTUP_AUTO_MIGRATE` | `true` | Apply EF migrations on API start |
| `STARTUP_SEED_IDENTITY` | `true` | Seed roles + admin user (idempotent) |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Disables Swagger/Hangfire dashboard |

Set `STARTUP_SEED_IDENTITY=false` after first deploy if you prefer.

### 6. Persistent data

Docker volumes:

- `patsanstha_pg` — PostgreSQL data
- `patsanstha_redis` — Redis data
- `patsanstha_uploads` — Member document uploads

---

## Build & run

All commands assume you are in the `NBFC/` directory.

### 1. Restore & build backend

```bash
dotnet restore Patsanstha.sln
dotnet build Patsanstha.sln
dotnet test Patsanstha.sln
```

### 2. Run the API

```bash
dotnet run --project src/Host/Patsanstha.Api
```

On first run in Development, the API will:

- Apply EF Core migrations for all module schemas
- Seed default admin user
- Start Hangfire background jobs

**Stop the API before rebuilding** if you see file-lock errors (MSB3026/MSB3027).

Tip: use watch mode during development:

```bash
dotnet watch run --project src/Host/Patsanstha.Api
```

### 3. Run the gateway (optional)

```bash
dotnet run --project src/Host/Patsanstha.Gateway
```

### 4. Run the frontend (optional)

```bash
cd frontend
npm install
npm start              # shell only (port 4200)
# or
npm run start:all      # shell + all MFEs (parallel)
```

---

## URLs & credentials

| Service | URL |
|---------|-----|
| API | http://localhost:5080 |
| Swagger (Development) | http://localhost:5080/swagger |
| Health (live) | http://localhost:5080/health/live |
| Health (ready) | http://localhost:5080/health/ready |
| Hangfire dashboard (Development) | http://localhost:5080/hangfire |
| Gateway | http://localhost:5000 |
| Angular shell | http://localhost:4200 |

**Default admin (Development seed):**

- Email: `admin@patsanstha.local`
- Password: `ChangeMe@123`

**Auth endpoints:**

| Method | Path | Auth |
|--------|------|------|
| POST | `/api/v1/auth/login` | Anonymous |
| POST | `/api/v1/auth/refresh` | Anonymous |
| POST | `/api/v1/auth/logout` | Bearer |
| GET | `/api/v1/auth/me` | Bearer |
| GET | `/api/v1/admin/roles` | `admin.roles.manage` |
| GET | `/api/v1/admin/permissions` | `admin.roles.manage` |

---

## Configuration files

| File | Purpose |
|------|---------|
| `.env.example` | Template for VPS/Docker secrets (copy to `.env`) |
| `src/Host/Patsanstha.Api/appsettings.json` | Connection strings, JWT, Hangfire cron schedules |
| `src/Host/Patsanstha.Api/appsettings.Development.json` | Dev overrides (Redis disabled, console telemetry) |
| `src/Host/Patsanstha.Gateway/appsettings.json` | YARP proxy routes |
| `docker-compose.yml` | Postgres + Redis for local infra |

---

## Troubleshooting

| Problem | Likely cause | Fix |
|---------|--------------|-----|
| `relation "identity.roles" does not exist` | DB existed before migrations | Drop/recreate `patsanstha`, or run EF migrations manually |
| Build fails with file locked | API still running | Stop `dotnet run`, then rebuild |
| `docker: command not found` | Docker not installed | Use local PostgreSQL (Option B) |
| Frontend CORS errors | API not running or wrong port | Start API on `:5080`; CORS allows `:4200` in Development |

### Manual EF migration (example — Identity)

```bash
dotnet ef migrations add InitialIdentity \
  --project src/Modules/Identity/Identity.Infrastructure \
  --startup-project src/Host/Patsanstha.Api \
  --output-dir Persistence/Migrations
```

---

## Design principles (non-negotiable)

- Money: `NUMERIC(18,2)` / `decimal` only — never `float`/`double`
- Business logic in domain aggregates + command handlers — not in controllers or Angular components
- Cross-module communication only via integration events + outbox
- One Postgres schema per module; no cross-schema FKs
- Idempotency-Key on every state-changing endpoint
- Never cache balances or EMI due amounts

---

## Further reading

- Module conventions: `src/Modules/README.md`
- UI reference mockups: `Design-Screens/`
