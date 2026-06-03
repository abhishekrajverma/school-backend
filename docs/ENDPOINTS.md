# API ↔ Frontend mapping

Base URL: `/api` (BFF: `/api/proxy/...` on Next.js)

## Auth (`lib/api/auth.ts` → BFF `/api/auth/*` → ASP.NET)

| Frontend | ASP.NET |
|----------|---------|
| `POST /api/auth/login` (BFF) | `POST /api/auth/login` |
| `POST /api/auth/refresh` | `POST /api/auth/refresh` |
| `GET /api/auth/me` | `GET /api/auth/me` |
| `POST /api/auth/logout` | `POST /api/auth/logout` |

## Tenancy (SaaS)

| Frontend (planned) | ASP.NET | Status |
|--------------------|---------|--------|
| `POST /tenants/provision` | `POST /api/tenants/provision` | Done |
| `GET /tenants/by-slug/{slug}` | `GET /api/tenants/by-slug/{slug}` | Done |
| `GET /tenants/current` | `GET /api/tenants/current` | Done |
| `GET /tenants/{id}` | `GET /api/tenants/{id}` | Done |

## Students (`lib/api/students.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /students` | `GET /api/students` | Done |
| `GET /students/{id}` | `GET /api/students/{id}` | Done |
| `POST /students` | `POST /api/students` | Done |
| `PUT /students/{id}` | `PUT /api/students/{id}` | Done |
| `DELETE /students/{id}` | `DELETE /api/students/{id}` | Done |

## Teachers (`lib/api/teachers.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /teachers` | `GET /api/teachers` | Done |
| `GET /teachers/{id}` | `GET /api/teachers/{id}` | Done |
| `POST /teachers` | `POST /api/teachers` | Done |
| `PUT /teachers/{id}` | `PUT /api/teachers/{id}` | Done |
| `DELETE /teachers/{id}` | `DELETE /api/teachers/{id}` | Done |

## Parents (`lib/api/parents.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /parents` | `GET /api/parents` | Done |
| `GET /parents/{id}` | `GET /api/parents/{id}` | Done |
| `POST /parents` | `POST /api/parents` | Done |
| `PUT /parents/{id}` | `PUT /api/parents/{id}` | Done |
| `DELETE /parents/{id}` | `DELETE /api/parents/{id}` | Done |

## Academics (`lib/api/academics.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /academics/classes` | `GET /api/academics/classes` | Done |
| `POST /academics/classes` | `POST /api/academics/classes` | Done |
| `GET /academics/subjects?className=` | `GET /api/academics/subjects` | Done |
| `POST /academics/subjects` | `POST /api/academics/subjects` | Done |

## Admissions (`lib/api/admissions.ts`, `lib/admission/*`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /admissions` | `GET /api/admissions?status=` | Done |
| `GET /admissions/{id}` | `GET /api/admissions/{id}` | Done |
| `POST /admissions` | `POST /api/admissions` | Done (anonymous + `X-Tenant-Id`) |
| `PUT /admissions/{id}` | `PUT /api/admissions/{id}` | Done (draft only) |
| `POST /admissions/{id}/submit` | `POST /api/admissions/{id}/submit` | Done |
| `PATCH /admissions/{id}/status` | `PATCH /api/admissions/{id}/status` | Done (admin) |
| `POST /admissions/{id}/documents` | `POST /api/admissions/{id}/documents` | Done (metadata) |

## Attendance (`lib/api/attendance.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /attendance` | `GET /api/attendance?date=&entityType=&className=` | Done |
| `GET /attendance/students/{studentId}` | `GET /api/attendance/students/{studentId}?from=&to=` | Done |
| `GET /attendance/{id}` | `GET /api/attendance/{id}` | Done |
| `POST /attendance` | `POST /api/attendance` | Done (mark single) |
| `POST /attendance/bulk` | `POST /api/attendance/bulk` | Done |

## Fees & payments (`lib/api/fees.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /fees` | `GET /api/fees?status=&studentId=` | Done |
| `GET /fees/{id}` | `GET /api/fees/{id}` | Done |
| `POST /fees` | `POST /api/fees` | Done |
| `POST /fees/{id}/payments` | `POST /api/fees/{id}/payments` | Done |
| `GET /payments` | `GET /api/payments?studentId=` | Done |

## Exams (`lib/api/exams.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /exams` | `GET /api/exams?className=&status=` | Done |
| `GET /exams/{id}` | `GET /api/exams/{id}` | Done |
| `POST /exams` | `POST /api/exams` | Done |
| `PUT /exams/{id}` | `PUT /api/exams/{id}` | Done |
| `DELETE /exams/{id}` | `DELETE /api/exams/{id}` | Done |

## Timetable (`lib/api/timetable.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /timetable` | `GET /api/timetable?className=&day=` | Done |
| `GET /timetable/{id}` | `GET /api/timetable/{id}` | Done |
| `PUT /timetable` | `PUT /api/timetable` (upsert by class + day) | Done |

## Notifications (`lib/api/notifications.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /notifications` | `GET /api/notifications?targetAudience=` | Done |
| `GET /notifications/{id}` | `GET /api/notifications/{id}` | Done |
| `POST /notifications` | `POST /api/notifications` | Done |
| `POST /notifications/{id}/read` | `POST /api/notifications/{id}/read` | Done |

## Payroll (`lib/api/payroll.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /payroll` | `GET /api/payroll?month=&year=&status=&employeeId=` | Done |
| `GET /payroll/{id}` | `GET /api/payroll/{id}` | Done |
| `POST /payroll` | `POST /api/payroll` | Done |
| `POST /payroll/{id}/process` | `POST /api/payroll/{id}/process` | Done |

## Leave (`lib/api/leave.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /leave-requests` | `GET /api/leave-requests` | Done |
| `GET /leave-requests/{id}` | `GET /api/leave-requests/{id}` | Done |
| `POST /leave-requests` | `POST /api/leave-requests` | Done |
| `POST /leave-requests/{id}/approve` | `POST /api/leave-requests/{id}/approve` | Done |
| `POST /leave-requests/{id}/reject` | `POST /api/leave-requests/{id}/reject` | Done |

## Library (`lib/api/library.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /library/books` | `GET /api/library/books` | Done |
| `POST /library/books` | `POST /api/library/books` | Done |
| `PUT /library/books/{id}` | `PUT /api/library/books/{id}` | Done |
| `DELETE /library/books/{id}` | `DELETE /api/library/books/{id}` | Done |
| `GET /library/issues` | `GET /api/library/issues` | Done |
| `POST /library/issues` | `POST /api/library/issues` | Done |
| `POST /library/issues/{id}/return` | `POST /api/library/issues/{id}/return` | Done |

## Transport (`lib/api/transport.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /transport/vehicles` | `GET /api/transport/vehicles` | Done |
| `POST /transport/vehicles` | `POST /api/transport/vehicles` | Done |
| `GET /transport/routes` | `GET /api/transport/routes` | Done |
| `POST /transport/routes` | `POST /api/transport/routes` | Done |

## Hostel (`lib/api/hostel.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /hostel/rooms` | `GET /api/hostel/rooms` | Done |
| `POST /hostel/rooms` | `POST /api/hostel/rooms` | Done |
| `GET /hostel/allocations` | `GET /api/hostel/allocations` | Done |
| `POST /hostel/allocations` | `POST /api/hostel/allocations` | Done |

## Inventory (`lib/api/inventory.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /inventory/items` | `GET /api/inventory/items` | Done |
| `POST /inventory/items` | `POST /api/inventory/items` | Done |
| `PUT /inventory/items/{id}` | `PUT /api/inventory/items/{id}` | Done |

## Dashboard & reports (`lib/api/dashboard.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /dashboard` | `GET /api/dashboard` | Done |
| `GET /reports` | `GET /api/reports?type=&from=&to=` | Done |

## Portals

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /students/me/*` | `GET /api/students/me`, `/fees`, `/attendance`, `/exams`, `/timetable`, `/library/issues` | Done |
| `GET /teachers/me/*` | `GET /api/teachers/me`, `/leaves`, `/payroll`, `/timetable` | Done |
| `GET /parents/me/*` | `GET /api/parents/me`, `/children`, `/children/{id}/fees`, `/children/{id}/attendance` | Done |

## Uploads (`lib/api/uploads.ts`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `POST /uploads` | `POST /api/uploads` (multipart `file`, optional `category`) | Done |
| `GET /uploads/{id}` | `GET /api/uploads/{id}` | Done |
| `GET /uploads/{id}/download` | `GET /api/uploads/{id}/download` | Done |

Response: `{ id, url, fileName, contentType, size }` — use `url` for download link.

## Transport assignments

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /transport/assignments` | `GET /api/transport/assignments` | Done |
| `GET /transport/assignments/{id}` | `GET /api/transport/assignments/{id}` | Done |
| `POST /transport/assignments` | `POST /api/transport/assignments` | Done |
| `GET /parents/me/children/{id}/transport` | `GET /api/parents/me/children/{id}/transport` | Done |

## Report export

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /reports/export` | `GET /api/reports/export?format=csv&type=fees\|attendance\|payroll` | Done |

Returns CSV file download.

## Background jobs

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /jobs/runs` | `GET /api/jobs/runs?jobType=` | Done |
| `POST /jobs/fee-reminders` | `POST /api/jobs/fee-reminders` (admin/principal) | Done |

Scheduled: `FeeReminderBackgroundService` runs every 24h (config `ScheduledJobs:FeeReminderIntervalHours`), creates parent notifications for pending/overdue fees.

## Report export (PDF)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /reports/export?format=pdf&type=fees` | Same with `format=pdf` | Done |

Types: `fees`, `attendance`, `payroll`. Formats: `csv`, `pdf`.

## Bulk import (`/api/imports`)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /imports/students/template` | CSV template download | Done |
| `POST /imports/students` | Sync CSV import (multipart `file`) | Done |
| `POST /imports/students/queue` | Async via Hangfire (`{ fileId }` from prior upload) | Done |
| `GET /imports/teachers/template` | CSV template | Done |
| `POST /imports/teachers` | Sync CSV import | Done |
| `POST /imports/teachers/queue` | Async via Hangfire | Done |

## Hangfire

| URL | Notes |
|-----|--------|
| `/hangfire` | Job dashboard (dev: open; prod: admin/principal JWT) |
| Recurring `fee-reminders-all-tenants` when `ScheduledJobs:UseHangfire` is true |

## Blob storage

Set `Uploads:Provider` to `Azure` and `Uploads:AzureConnectionString` + `AzureContainer` for Azure Blob; `S3` for MinIO/AWS; default `Local` uses `uploads/` folder.

## SignalR (live notifications)

| URL | Notes |
|-----|--------|
| `/hubs/notifications` | WebSocket; query `access_token`, `tenant_id`; event `notification.created` |

Pushed when notifications are created (API or fee-reminder job). Redis backplane when `Redis:Enabled` is true.

## Integration events / outbox

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /events/outbox` | `GET /api/events/outbox?status=pending` | Done |
| `GET /region` | `GET /api/region` | Done |

## API Gateway

| URL | Notes |
|-----|--------|
| `http://localhost:5100/api/*` | YARP reverse proxy to API (port 5000); forwards tenant/region/auth headers |

## OpenTelemetry

Set `OpenTelemetry:OtlpEndpoint` for OTLP export; console exporter when unset (development).

## API versioning (Phase 9)

| Path | Notes |
|------|--------|
| `/api/*` | Current stable routes |
| `/api/v1/*` | Versioned alias (same handlers) |
| `GET /api/version` | Supported versions |
| Header `X-Api-Version` | Optional; default `1.0` |

## Audit

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /audit/logs` | `GET /api/audit/logs` | Done |

## Webhooks

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /webhooks` | `GET /api/webhooks` | Done |
| `POST /webhooks` | `POST /api/webhooks` | Done |
| `DELETE /webhooks/{id}` | `DELETE /api/webhooks/{id}` | Done |
| `GET /webhooks/deliveries` | `GET /api/webhooks/deliveries` | Done |

## Chaos engineering

| Path | Notes |
|------|--------|
| `GET /api/chaos/config` | View chaos settings (admin) |

Enable via `Chaos:Enabled` in config (blocked in production unless `AllowInProduction`).

## OIDC / SSO (Phase 10)

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /auth/oidc/config` | `GET /api/auth/oidc/config` | Done |
| `POST /auth/oidc/login` | `POST /api/auth/oidc/login` `{ idToken }` | Done |

## Data retention

| Frontend proxy path | ASP.NET | Status |
|---------------------|---------|--------|
| `GET /retention/policies` | `GET /api/retention/policies` | Done |
| `PUT /retention/policies` | `PUT /api/retention/policies` | Done |
| `POST /retention/run` | `POST /api/retention/run` (manual purge) | Done |

## GraphQL (read-only)

| URL | Notes |
|-----|--------|
| `POST /graphql` | HotChocolate; Bearer + `X-Tenant-Id` required |

Queries: `students`, `studentById`, `dashboard`.

## Redis & read replica

| Config | Purpose |
|--------|---------|
| `Redis:Enabled` | Distributed cache + rate limit + SignalR scale-out |
| `ConnectionStrings:ReadConnection` | SQL read replica for heavy list/report/dashboard queries |
| `Database:UseReadReplica` | Route read factory to replica connection |
