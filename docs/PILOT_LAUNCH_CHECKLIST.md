# Pilot launch checklist — 10 schools

Use after **local smoke test passes**, before onboarding real schools.

---

## Phase A — Local verification (you)

- [ ] Docker Desktop running
- [ ] `docker compose up -d sqlserver` → container **healthy**
- [ ] `dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api`
- [ ] `dotnet run --project src/EduSync.Api` → http://localhost:5000/swagger
- [ ] `.\scripts\local\smoke-test.ps1` → **all PASS**

Manual Swagger checks:

- [ ] `POST /api/auth/login` (admin) → token + `permissions[]`
- [ ] `GET /api/students` + `X-Tenant-Id: demo-school-001` → 200
- [ ] Student login → `GET /api/students` → **403**, `GET /api/students/me` → **200**

---

## Phase B — Azure (single database)

- [ ] Run `scripts/azure/setup-sql-database.ps1` (see [AZURE_DATABASE_SETUP.md](AZURE_DATABASE_SETUP.md))
- [ ] Migrations against Azure SQL
- [ ] App Service (Linux P1v3 × 3–4 instances) + connection string in Configuration
- [ ] Redis Standard C2 enabled (`Redis:Enabled: true`)
- [ ] `Capacity:SingleDatabase: true`, `UseReadReplica: false`
- [ ] JWT `Key` rotated (32+ chars, Key Vault)
- [ ] CORS: production dashboard URL in `Cors:Origins`

---

## Phase C — Dashboard integration

- [ ] `school-erp-dashboard` `.env.local`: `NEXT_PUBLIC_USE_MOCK=false`
- [ ] `NEXT_PUBLIC_API_URL` → deployed API or gateway
- [ ] Login flow works with real token
- [ ] Admin: students list/create/edit
- [ ] Teacher: attendance or exams (one flow)
- [ ] Student/parent portal pages load

---

## Phase D — Per school onboarding (×10)

For each school:

- [ ] `POST /api/tenants/provision` (or admin UI) — school name, slug, admin email
- [ ] Admin receives credentials / sets password
- [ ] `X-Tenant-Id` = school external id or slug documented for that school
- [ ] Import students/teachers CSV if bulk load needed (`/api/imports/*`)
- [ ] Smoke: one admin login + one student/parent portal login

---

## Phase E — Pilot week monitoring

- [ ] Application Insights: errors & p95 latency
- [ ] SQL CPU &lt; 70% at peak
- [ ] No sustained 429 rate limits (tune `RateLimitPerMinute` if needed)
- [ ] Hangfire `/hangfire` — fee reminder jobs succeeding
- [ ] Backup: Azure SQL PITR enabled (default)

---

## Demo accounts (dev only)

Remove or change default passwords before real schools:

| Email | Role |
|-------|------|
| admin@school.edu | admin |
| arjun.s@school.edu | student |

---

## Scale trigger (school 11+)

When adding schools beyond 10 or SQL CPU &gt; 70%:

1. SQL 2→4 vCore
2. API +1–2 instances
3. See [CAPACITY.md](CAPACITY.md)

---

## Quick commands

```powershell
# Local API
dotnet run --project src/EduSync.Api

# Smoke test
.\scripts\local\smoke-test.ps1

# Migrate Azure
$env:ConnectionStrings__DefaultConnection = "Server=tcp:..."
dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api
```
