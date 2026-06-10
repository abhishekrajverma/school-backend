# EduSync School ERP Backend

ASP.NET Core **modular monolith** for the [EduSync](https://github.com) Next.js frontend (`school-erp-dashboard`). Multi-tenant SaaS with shared SQL Server schema, JWT auth, and tenant-scoped EF Core queries.

**Stack:** .NET 10, EF Core 10, SQL Server 2022, MediatR, FluentValidation, Serilog, Swagger.

## Quick start

**Full local guide:** [docs/LOCAL_SETUP.md](docs/LOCAL_SETUP.md) (Docker SQL → migrations → Swagger → demo login).

### 1. SQL Server + API (Docker)

```bash
docker compose up -d sqlserver
# wait ~30s for SQL to be healthy
dotnet run --project src/EduSync.Api/EduSync.Api.csproj
```

API: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### 2. Connect the Next.js frontend

In `school-erp-dashboard/.env.local`:

```env
NEXT_PUBLIC_USE_MOCK=false
NEXT_PUBLIC_API_URL=http://localhost:5000/api
API_URL=http://localhost:5000/api
```

Restart `pnpm dev`. The BFF proxies `/api/proxy/*` → ASP.NET `/api/*`.

### Demo tenant

| Field | Value |
|-------|--------|
| Tenant ID (header) | `demo-school-001` |
| Slug | `demo-school` |
| Admin | `admin@school.edu` / `admin123` |

## Solution layout

```
src/
  EduSync.Api/              Host, endpoints, Swagger
  EduSync.SharedKernel/     Pagination, Result, tenant abstractions
  EduSync.Infrastructure/   DbContext, middleware, handlers, migrations
  Modules/
    EduSync.Modules.Identity/
    EduSync.Modules.Tenancy/
    EduSync.Modules.Students/
    EduSync.Modules.Staff/      (Teachers)
    EduSync.Modules.Parents/
    EduSync.Modules.Academics/
    EduSync.Modules.Admissions/
    EduSync.Modules.Attendance/
    EduSync.Modules.Fees/
    EduSync.Modules.Exams/
    EduSync.Modules.Assignments/
    EduSync.Modules.Timetable/
    EduSync.Modules.Notifications/
    EduSync.Modules.Payroll/
    EduSync.Modules.Leave/
    EduSync.Modules.Library/
    EduSync.Modules.Transport/
    EduSync.Modules.Hostel/
    EduSync.Modules.Inventory/
    EduSync.Modules.Dashboard/
    EduSync.Modules.Portals/
    EduSync.Modules.Uploads/
    EduSync.Modules.Jobs/
    EduSync.Modules.Imports/
    EduSync.Modules.Events/
    EduSync.Modules.Audit/
    EduSync.Modules.Webhooks/
    EduSync.Gateway/          YARP reverse proxy (port 5100)
tests/
    EduSync.ArchitectureTests/   NetArchTest layer rules
    EduSync.UnitTests/
    EduSync.IntegrationTests/

GraphQL: http://localhost:5000/graphql
```

Hangfire dashboard: http://localhost:5000/hangfire (when `ScheduledJobs:UseHangfire` is true).

## Phase 3 endpoints (implemented)

| Module | Base path |
|--------|-----------|
| Attendance | `/api/attendance` — list, student history, mark, bulk mark |
| Fees | `/api/fees` — list, get, create, record payment |
| Payments | `/api/payments` — paginated payment history |
| Exams | `/api/exams` — full CRUD |
| Timetable | `/api/timetable` — list, get, upsert (PUT) |
| Notifications | `/api/notifications` — list, create, mark read |

**Apply migration after pull:**

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

Restart API — existing demo DBs auto-seed Phase 3 data if the attendance table is empty.

## Phase 4 endpoints (implemented)

| Module | Base path |
|--------|-----------|
| Payroll | `/api/payroll` — list, create, process |
| Leave | `/api/leave-requests` — list, create, approve, reject |
| Library | `/api/library/books`, `/api/library/issues` |
| Transport | `/api/transport/vehicles`, `/api/transport/routes` |
| Hostel | `/api/hostel/rooms`, `/api/hostel/allocations` |
| Inventory | `/api/inventory/items` |
| Dashboard | `/api/dashboard` — KPIs and chart data |
| Reports | `/api/reports?type=` — tabular exports |
| Student portal | `/api/students/me/*` |
| Teacher portal | `/api/teachers/me/*` |
| Parent portal | `/api/parents/me/*` |

**Apply migration after pull:**

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

Restart API — existing demo DBs auto-seed Phase 4 data if the payroll table is empty.

Portal logins (demo): student `arjun.s@school.edu` / `student123`, teacher `anita.s@school.edu` / `teacher123`, parent `rajesh.sharma@email.com` / `parent123`.

## Phase 2 endpoints (implemented)

| Module | Base path |
|--------|-----------|
| Teachers (Staff) | `/api/teachers` — full CRUD |
| Parents | `/api/parents` — full CRUD |
| Academics | `/api/academics/classes`, `/api/academics/subjects` |
| Admissions | `/api/admissions` — create/update/submit/status/documents |

Admission wizard: store full form JSON per step; public create/update/submit with `X-Tenant-Id: demo-school-001` (no auth). Admin list/status requires JWT.

**Apply migration after pull:**

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

Restart API — existing demo DBs auto-seed Phase 2 data if teachers table is empty.

## Phase 1 endpoints (implemented)

| Method | Path | Notes |
|--------|------|--------|
| POST | `/api/auth/login` | Returns `{ accessToken, refreshToken, expiresIn, user }` |
| POST | `/api/auth/refresh` | Rotating refresh token |
| GET | `/api/auth/me` | Requires JWT + tenant header |
| POST | `/api/auth/logout` | Revokes refresh token(s) |
| POST | `/api/tenants/provision` | SaaS onboarding |
| GET | `/api/tenants/by-slug/{slug}` | Public branding |
| GET | `/api/tenants/current` | Authenticated tenant |
| GET | `/api/tenants/{id}` | By external id or slug |
| GET/POST/PUT/DELETE | `/api/students` | Full CRUD, paginated list |
| GET | `/api/health` | Health check |

Use `EduSync.http` or Swagger for examples.

## Multi-tenancy & request context

| Header | Purpose |
|--------|---------|
| `X-Tenant-Id` | Required for tenant APIs (GUID, external id, or slug e.g. `demo-school-001`) |
| `X-Branch-Id` | Optional branch (external id or code); scopes branch-bound data |
| `X-Financial-Year` or `X-Academic-Year-Id` | Academic year name or GUID; defaults to tenant’s current year |
| `Authorization: Bearer` | JWT from `/api/auth/login` |

**Pipeline (after auth):** `TenantRateLimit` → `TenantResolution` → `TenantAuthorization` → `BranchResolution` → `BranchAuthorization` → `AcademicYearResolution` → `AuditLogging`

1. `TenantResolutionMiddleware` resolves tenant and sets `ITenantContext`.
2. `TenantAuthorizationMiddleware` validates JWT `tenant_id` matches header and loads role from `identity.TenantMemberships` (not JWT role alone).
3. `BranchResolutionMiddleware` + `BranchAuthorizationMiddleware` enforce `identity.BranchMemberships` when `X-Branch-Id` is set (admin/principal have tenant-wide branch access).
4. `AcademicYearResolutionMiddleware` validates academic year against `tenancy.AcademicYears`.
5. EF global filters: `TenantId` on all tenant entities; `BranchId` on `BranchEntity` tables when branch context is resolved.

## Migrations

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

## Tests

```bash
dotnet test tests/EduSync.UnitTests          # Role permissions, etc.
dotnet test tests/EduSync.ArchitectureTests  # NetArchTest layer rules
dotnet test tests/EduSync.IntegrationTests   # Requires Docker (Testcontainers)
```

## Configuration

Copy `src/EduSync.Api/appsettings.Development.json` and set:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key` (32+ chars in production)

## Docs

- **[docs/CODEBASE_GUIDE.md](docs/CODEBASE_GUIDE.md)** — **Folder structure, diagrams, “where to change” lookup** (start here when editing code)
- [docs/ENDPOINTS.md](docs/ENDPOINTS.md) — Frontend route mapping
- [docs/SCHEMA.md](docs/SCHEMA.md) — SQL schemas per module
- [docs/ERD.md](docs/ERD.md) — **Entity relationship diagrams (data model)**
- [docs/LOCAL_SETUP.md](docs/LOCAL_SETUP.md) — **Test on your PC first (Docker SQL)**
- [docs/PILOT_LAUNCH_CHECKLIST.md](docs/PILOT_LAUNCH_CHECKLIST.md) — **10-school pilot go-live checklist**
- [docs/AZURE_FREE_SQL_SETUP.md](docs/AZURE_FREE_SQL_SETUP.md) — **Azure free SQL offer (portal) + connect EduSync**
- [docs/AZURE_DATABASE_SETUP.md](docs/AZURE_DATABASE_SETUP.md) — **Paid Azure SQL (production / 10+ schools)**
- [docs/CAPACITY.md](docs/CAPACITY.md) — **200 schools × 300 concurrent, single-database production profile**
- [docs/SCALING.md](docs/SCALING.md) — Scale-out and extraction notes

## Phase 5 endpoints (implemented)

| Module | Base path |
|--------|-----------|
| Uploads | `/api/uploads` — multipart upload, metadata, download |
| Transport assignments | `/api/transport/assignments` |
| Report export | `/api/reports/export?format=csv&type=fees` |
| Jobs | `/api/jobs/runs`, `POST /api/jobs/fee-reminders` |
| Parent transport | `/api/parents/me/children/{id}/transport` |

**Apply migration after pull:**

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

Restart API — demo DBs auto-seed transport assignments if that table is empty.

**Config** (`appsettings.Development.json`):

- `Uploads:RootPath` — local folder for files (default `uploads/`)
- `ScheduledJobs:Enabled` — set `false` to disable automatic fee reminders
- `ScheduledJobs:FeeReminderIntervalHours` — default `24`

## Phase 6 endpoints (implemented)

| Feature | Details |
|---------|---------|
| Hangfire | Dashboard at `/hangfire`; recurring fee reminders when `ScheduledJobs:UseHangfire: true` |
| PDF reports | `GET /api/reports/export?format=pdf&type=fees` |
| Azure uploads | `Uploads:Provider: Azure` + connection string |
| Bulk import | `POST /api/imports/students`, `/teachers` (CSV); `/queue` for Hangfire async |

**Hangfire:** Creates `hangfire` schema in SQL Server on first run. Disable legacy scheduler: set `UseHangfire: true` (default in Development).

**Azure example:**

```json
"Uploads": {
  "Provider": "Azure",
  "AzureConnectionString": "<storage-connection-string>",
  "AzureContainer": "edusync-uploads"
}
```

## Phase 7 endpoints (implemented)

| Feature | Details |
|---------|---------|
| SignalR | Hub `/hubs/notifications` — event `notification.created` (JWT via `?access_token=` or header) |
| Redis | Tenant lookup cache, dashboard KPI cache, per-tenant rate limit, SignalR backplane |
| Read replica | `ConnectionStrings:ReadConnection` + `Database:UseReadReplica: true` for list/report/dashboard queries |
| S3 uploads | `Uploads:Provider: S3` with `S3ServiceUrl` (MinIO), keys, bucket |

**SignalR client (browser):**

```js
import * as signalR from "@microsoft/signalr";
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/hubs/notifications?access_token=" + token + "&tenant_id=demo-school-001")
  .withAutomaticReconnect()
  .build();
connection.on("notification.created", (n) => console.log(n));
await connection.start();
```

**Redis (local):** `docker compose up -d redis` then set `Redis:Enabled: true` in appsettings.

**MinIO / S3 example:**

```json
"Uploads": {
  "Provider": "S3",
  "S3ServiceUrl": "http://localhost:9000",
  "S3AccessKey": "minioadmin",
  "S3SecretKey": "minioadmin",
  "S3Bucket": "edusync-uploads"
}
```

## Phase 8 endpoints (implemented)

| Feature | Details |
|---------|---------|
| API Gateway | YARP project `EduSync.Gateway` on port **5100** → proxies `/api`, `/hubs`, `/hangfire` |
| Outbox / events | `events.Messages` table; `GET /api/events/outbox`; dispatcher processes pending messages |
| OpenTelemetry | Traces + metrics (ASP.NET, EF, HTTP); OTLP when `OpenTelemetry:OtlpEndpoint` set |
| Multi-region | `X-Region` header; `GET /api/region`; default `ap-south-1` |

**Gateway (local):**

```bash
dotnet run --project src/EduSync.Gateway/EduSync.Gateway.csproj
# Frontend: NEXT_PUBLIC_API_URL=http://localhost:5100/api
```

**Outbox events:** `notification.created`, `student.created`, `fee.payment.recorded` (queued on save, async dispatch every 5s).

**OTLP (Jaeger / Grafana):**

```json
"OpenTelemetry": {
  "Enabled": true,
  "OtlpEndpoint": "http://localhost:4317"
}
```

**Apply migration:**

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

## Phase 9 endpoints (implemented)

| Feature | Details |
|---------|---------|
| API versioning | `/api` and `/api/v1` (same routes); `GET /api/version`; header `X-Api-Version: 1.0` |
| Audit log | `GET /api/audit/logs` — HTTP write audit trail per tenant |
| Webhooks | `GET/POST/DELETE /api/webhooks`; deliveries on outbox events with HMAC signature |
| Chaos testing | `Chaos:Enabled` injects latency/failures (dev only by default); `GET /api/chaos/config` |

**Webhooks:** Subscribe to `notification.created`, `student.created`, `fee.payment.recorded` (or `*`). Delivered when outbox processes events.

**Chaos (local stress test):**

```json
"Chaos": { "Enabled": true, "FailureRate": 0.1, "MaxLatencyMs": 300 }
```

**Apply migration:**

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

## Phase 10 endpoints (implemented)

| Feature | Details |
|---------|---------|
| SSO / OIDC | `GET /api/auth/oidc/config`, `POST /api/auth/oidc/login` with `{ idToken }` → EduSync JWT |
| Field encryption | AES-GCM for student email/phone/address (when `Encryption:Enabled`) |
| Data retention | `GET/PUT /api/retention/policies`, `POST /api/retention/run`; nightly cleanup job |
| GraphQL | Read API at `/graphql` — `students`, `studentById`, `dashboard` (requires JWT + tenant) |

**OIDC flow:** Frontend completes OIDC with your IdP → send `idToken` to `/api/auth/oidc/login` → receive same tokens as password login (user must exist by email).

**Encryption key (32 bytes, base64):**

```bash
# PowerShell: [Convert]::ToBase64String((1..32|ForEach-Object { Get-Random -Max 256 }))
```

**GraphQL example:**

```graphql
query {
  students(page: 1, pageSize: 10) { id fullName className email }
  dashboard { stats { students teachers } }
}
```

Headers: `Authorization: Bearer <token>`, `X-Tenant-Id: demo-school-001`

**Apply migration:**

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```

## Phase 11 — ERP architecture remediation (implemented)

Core multi-branch ERP domain: student master + enrollments, registration/admission workflow, promotion, branch RBAC foundations, integration events.

| Feature | API / behavior |
|---------|----------------|
| Branches | `GET/POST/PATCH /api/branches` |
| Branch memberships | `GET/POST/DELETE /api/branches/{id}/memberships` |
| Registrations | `GET/POST/PUT /api/registrations`, submit, convert → admission |
| Admission approve | `POST /api/admissions/{id}/approve` → student + enrollment + outbox events |
| Student enrollments | Class/section/roll on `students.Enrollments` per academic year (not on `Students` row) |
| Promotion | `POST /api/promotions/bulk`, rollback |
| Academic year context | Header `X-Academic-Year-Id` / `X-Financial-Year` |
| Security | Tenant JWT vs header match; parent portal IDOR fix; tenant-filtered outbox |

**Migrations:** `ErpArchitectureRemediation` — apply with `dotnet ef database update`.

## Phase 12 — Future phases (implemented)

DDD maturity, portal readiness, architecture governance.

| Feature | API / behavior |
|---------|----------------|
| Academic year CRUD | `POST /api/financial-year-settings/years`, `POST .../years/{id}/close` |
| Teacher assignments | `GET/POST/DELETE /api/teachers/assignments` |
| Exam results | `GET/POST /api/exams/results`; portal exams include marks/grade |
| Assignments module | `GET/POST /api/assignments`; portal `GET /students/me/assignments`, submit |
| Rich domain | `AdmissionApplication`, `Registration`, `Student`, `AcademicYear` partial classes with behavior methods |
| Architecture tests | `dotnet test tests/EduSync.ArchitectureTests` (NetArchTest layer rules) |

**Permissions added:** `assignments.read`, `assignments.write`

**Migration:** `FuturePhases` — `exams.ExamResults`, `assignments.Assignments`, `assignments.StudentAssignments`

```bash
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
dotnet test tests/EduSync.UnitTests tests/EduSync.ArchitectureTests
```

## Roadmap

Phases **1–12** are implemented. Further work is product-specific (production hardening, custom roles in SQL, frontend wiring for new endpoints, etc.).
