# Local testing (before Azure)

Run the API on your PC with **Docker SQL Server** (same as production schema, no Azure required).

---

## Prerequisites

| Tool | Check |
|------|--------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | `dotnet --version` |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | `docker --version` |
| (Optional) VS / VS Code | For debugging |

---

## Step 1 — Start SQL Server

### Option A — Docker (recommended)

1. **Start Docker Desktop** and wait until it says “Running”.
2. From the repo root:

```powershell
cd C:\Users\rajab\Desktop\school-erp-backend
docker compose up -d sqlserver
```

Wait until healthy (~30–60 seconds):

```powershell
docker compose ps
```

`sqlserver` should show **healthy**.

If you see `dockerDesktopLinuxEngine: The system cannot find the file` → Docker Desktop is **not running**; start it and retry.

### Option B — SQL Server / LocalDB already on Windows

Install [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or use **LocalDB**, then set the connection string in `appsettings.Development.json`:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EduSync;Trusted_Connection=True;TrustServerCertificate=True"
```

Or SSMS instance:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EduSync;Trusted_Connection=True;TrustServerCertificate=True"
```

Then continue from **Step 3** (migrations).

**Optional (Redis, SignalR backplane, rate-limit cache):**

```powershell
docker compose up -d redis
```

Then in `src/EduSync.Api/appsettings.Development.json` set `"Redis": { "Enabled": true, ... }`.  
For a quick test, **SQL only is enough** — leave `Redis:Enabled` as `false`.

---

## Step 2 — Connection string (already configured)

`appsettings.Development.json` points to:

```text
Server=localhost,1433;Database=EduSync;User Id=sa;Password=Your_strong_password123;...
```

This matches `docker-compose.yml`. No change needed unless you use a different SQL instance.

---

## Step 3 — Create / update database

```powershell
cd C:\Users\rajab\Desktop\school-erp-backend

dotnet ef database update `
  --project src/EduSync.Infrastructure `
  --startup-project src/EduSync.Api
```

First run creates all tables + indexes. Re-run after pulling new migrations.

---

## Step 4 — Run the API

```powershell
dotnet run --project src/EduSync.Api
```

| URL | Use |
|-----|-----|
| Swagger | http://localhost:5000/swagger |
| Health | http://localhost:5000/api/health |
| GraphQL | http://localhost:5000/graphql (if enabled) |
| Hangfire | http://localhost:5000/hangfire |

On startup, **SeedData** creates demo tenant and users if the DB is empty.

---

## Step 5 — Automated smoke test

With the API running in another terminal:

```powershell
.\scripts\local\smoke-test.ps1
```

Expect **all PASS** (health, admin login, students list, RBAC 403 for student, student portal).

---

## Step 6 — Test login (Swagger or curl)

**Headers for all tenant APIs:**

```http
X-Tenant-Id: demo-school-001
Authorization: Bearer {accessToken}
```

Optional (multi-branch / academic year):

```http
X-Branch-Id: {branch-external-id}
X-Financial-Year: 2025-26
# or
X-Academic-Year-Id: {guid}
```

**Login** (`POST /api/auth/login`):

```json
{
  "email": "admin@school.edu",
  "password": "admin123"
}
```

Copy `accessToken` from the response.

**Authorized requests:** Swagger → **Authorize** → Bearer `{accessToken}`  
Or header: `Authorization: Bearer {token}`

**Current user + permissions:** `GET /api/auth/me`

**Students (admin):** `GET /api/students?page=1&pageSize=20`

**Student role (should get 403 on admin list):** login `arjun.s@school.edu` / `student123` → `GET /api/students` → 403  
Portal: `GET /api/students/me` with same token.

---

## Step 7 — REST file (optional)

Open `EduSync.http` in VS / Rider / VS Code REST Client and run requests (set `@token` after login).

---

## Step 8 — Next.js dashboard (optional)

In `school-erp-dashboard/.env.local`:

```env
NEXT_PUBLIC_USE_MOCK=false
NEXT_PUBLIC_API_URL=http://localhost:5000/api
API_URL=http://localhost:5000/api
```

```bash
pnpm dev
```

---

## Demo accounts

| Role | Email | Password |
|------|--------|----------|
| Admin | admin@school.edu | admin123 |
| Principal | principal@school.edu | principal123 |
| Teacher | anita.s@school.edu | teacher123 |
| Student | arjun.s@school.edu | student123 |
| Parent | rajesh.sharma@email.com | parent123 |

Tenant header: **`demo-school-001`**

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Cannot connect to SQL | Docker running? `docker compose up -d sqlserver`; wait for healthy |
| Login fails | DB migrated? Seed ran? Check API console for errors |
| 403 on API | Missing `X-Tenant-Id: demo-school-001` or JWT tenant mismatch |
| 403 with branch header | User lacks `identity.BranchMemberships` for that branch (admin/principal bypass) |
| 403 as student on `/api/students` | Expected (RBAC); use `/api/students/me` |
| New tables missing | Run `dotnet ef database update` (migrations `ErpArchitectureRemediation`, `FuturePhases`) |
| Port 5000 in use | Change `launchSettings.json` or stop other app |
| Migration error | `dotnet ef database update` again; drop DB only if dev reset OK |

**Reset local database (destructive):**

```powershell
docker compose down -v
docker compose up -d sqlserver
# wait healthy, then dotnet ef database update + dotnet run
```

---

## When local works → Azure

Follow [AZURE_DATABASE_SETUP.md](AZURE_DATABASE_SETUP.md) and point `DefaultConnection` to Azure SQL instead of `localhost`.
