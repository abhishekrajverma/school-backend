# Capacity — 200 schools, single database

Deployment target:

| Setting | Value |
|---------|--------|
| Schools (tenants) | **200** |
| Peak concurrent users per school | **300** |
| Parent daily active users per school | **500** |
| Database | **One SQL Server** (`DefaultConnection` only) |

Configure in `appsettings.Production.json` → `Capacity` section (mirrored in Development for local testing).

---

## Architecture choices for this target

### Single database (required)

- `Capacity:SingleDatabase` = **true** forces all reads (including `IReadDbContextFactory`) to use **`DefaultConnection`** even if `ReadConnection` is set.
- `Database:UseReadReplica` must stay **false**.

Scale **horizontally at the API layer** (multiple `EduSync.Api` instances + load balancer + Redis). Do **not** split reads to a replica unless you change this policy.

### SQL connection pooling

Production connection string should include:

```text
Min Pool Size=50;Max Pool Size=500;Connect Timeout=30
```

200 schools × bursty traffic needs enough pooled connections across API replicas. Tune `Max Pool Size` with DBA after load tests.

### Redis (required in production)

| Setting | Recommended | Why |
|---------|-------------|-----|
| `Redis:Enabled` | `true` | Tenant lookup cache, dashboard cache, rate limits, SignalR backplane |
| `Redis:RateLimitPerMinute` | `8000` | ~300 concurrent users/school with SPA bursts (old default `300` was too low) |
| `Redis:DashboardCacheSeconds` | `90` | Reduces repeated aggregate queries on one DB |

### API replicas

Run **at least 4–8** API containers/VMs behind the gateway for production. Stateless JWT + shared SQL + shared Redis.

### Background work

- Fee reminders / CSV queue: **Hangfire** on dedicated worker(s) so CRUD paths stay responsive on the main API.

---

## Database optimizations included

Migration `Capacity_SingleDbIndexes` adds:

- `fees.Invoices`: `(TenantId, StudentExternalId)`, `(TenantId, Status)` + shorter string columns
- `fees.Payments`: `(TenantId, StudentExternalId)`
- `students.Students`: `(TenantId, Status)`
- `notifications.Notifications`: `(TenantId, TargetAudience)`

Global EF filters:

- **Tenant** isolation (`TenantId` from `X-Tenant-Id`)
- **Soft delete** (`!IsDeleted` on all `TenantEntity` types)

Student list search: minimum **2 characters**, uses `EF.Functions.Like` (less CPU than `ToLower().Contains` on every row).

SQL resilience: **retry on failure** (3×) and configurable **command timeout** (`Database:CommandTimeoutSeconds`).

---

## Expected user experience

| Scenario | With production checklist below |
|----------|----------------------------------|
| One school, 300 users online | CRUD and portal reads feel **fast** if SQL Server is sized (8+ vCPU, SSD) |
| 200 schools, staggered peaks | **Good** with 4–8 API instances + Redis |
| 200 schools, all peak same minute | Possible but run **load tests**; watch SQL CPU and connection pool |

Parents (500 DAU/school) are spread across the day — low average load; peaks align with fee/results portals.

---

## Production checklist

1. Set `ASPNETCORE_ENVIRONMENT=Production` and use `appsettings.Production.json`.
2. **Single DB**: `Capacity:SingleDatabase=true`, `Database:UseReadReplica=false`.
3. Enable **Redis** with `RateLimitPerMinute: 8000`.
4. Deploy **4+ API replicas** + YARP gateway (`EduSync.Gateway`).
5. Size **SQL Server** (monitor DTU/CPU, tempdb, log file).
6. Run migrations: `dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api`.
7. Load test with k6: 300 VUs per tenant, then 20 tenants in parallel (`docs/SCALING.md`).

---

## Load testing (k6 sketch)

```bash
# Per-tenant smoke (replace token + tenant)
k6 run --vus 300 --duration 5m scripts/load/login-and-students.js
```

Track p95 for:

- `POST /api/auth/login`
- `GET /api/students?page=1&pageSize=20`
- `GET /api/dashboard`

Targets (starting goals): p95 **&lt; 500ms** for lists, **&lt; 200ms** for cached dashboard.

---

## Related docs

- [AZURE_DATABASE_SETUP.md](AZURE_DATABASE_SETUP.md) — create Azure SQL, run migrations, connection string
- [SCALING.md](SCALING.md) — horizontal API scale, Redis, optional replica (off for your deployment)
- [CODEBASE_GUIDE.md](CODEBASE_GUIDE.md) — where to change handlers and config
