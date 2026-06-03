# Scaling runbook & microservice extraction

**Deployment target (200 schools / 300 concurrent per school / single SQL):** see [CAPACITY.md](CAPACITY.md). Use `Capacity:SingleDatabase=true` and scale **API + Redis**, not a read replica.

## Stateless API

- No in-memory session; JWT + refresh in SQL.
- Scale API pods behind a load balancer; sticky sessions not required.

## Redis (Phase 7)

- Tenant resolution cache (`ITenantLookupCache`).
- Dashboard KPI cache (60s TTL, configurable).
- Rate limit per `X-Tenant-Id` (`TenantRateLimitMiddleware`, default 300 req/min).
- SignalR backplane for `/hubs/notifications` when scaling API horizontally.

## SQL Server

- **Writes:** primary.
- **Reads:** `IReadDbContextFactory` targets `ReadConnection` when `Database:UseReadReplica` is true (students list, notifications, dashboard, reports export).
- Connection pooling via ADO.NET defaults; retry with Polly for transient errors.
- **Pagination:** mandatory on list endpoints; keyset pagination for large tables later.

## Background work

- Fee reminders: Hangfire recurring job when `ScheduledJobs:UseHangfire` is true; otherwise `FeeReminderBackgroundService` (Phase 5).
- Bulk CSV import: sync via `/api/imports/*` or queued via Hangfire (`/queue` + prior `/api/uploads`).

## Extracting a module to a microservice

1. Move `EduSync.Modules.{Name}` to its own solution with own DbContext **only for that schema**.
2. Replace cross-module FKs with integration events + outbox (`events.Messages`, Phase 8).
3. Expose HTTP/gRPC API; **EduSync.Gateway** (YARP) routes `/api/*` to the monolith or future services.
4. Keep `X-Tenant-Id` and JWT claims contract unchanged for the BFF.

## Load testing

- Target with k6: document p95 latency and RPS per tenant for `GET /students` and `POST /auth/login`.
