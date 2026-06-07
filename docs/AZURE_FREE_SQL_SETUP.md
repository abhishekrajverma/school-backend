# Azure **free** SQL Database — EduSync setup

For the portal flow: **Create SQL Database** → free offer → server `erp-sql-abhishek` (Central India).

> **Use for:** development, demos, light testing.  
> **Not for:** 10 schools × 200 concurrent users in production (upgrade to paid GP 2–4 vCore later).

---

## Part 1 — Finish creating the database (Portal)

### 1. Review + create

On your screen:

| Field | Your value | OK? |
|-------|------------|-----|
| Resource group | `ERP-SUITE-RG` | ✓ |
| Database name | `free-sql-db-6245011` | ✓ (or rename to `EduSync` if you prefer) |
| Server | `erp-sql-abhishek` (Central India) | ✓ |
| Free offer | Applied | ✓ |

Click **Review + create** → **Create** → wait until deployment succeeds.

### 2. Firewall (required)

1. Open resource **SQL server** `erp-sql-abhishek` (not only the database).
2. **Networking** (or **Firewalls and virtual networks**).
3. **Add your current client IPv4 address** → Save.
4. Turn on **Allow Azure services and resources to access this server** (needed when API runs on App Service later).

### 3. Get server admin credentials

You set these when you created `erp-sql-abhishek`:

- **Server admin login** (e.g. `sqladmin`)
- **Password**

If you forgot the password: server → **Reset password**.

---

## Part 2 — Create app user on the database

### Option A — Query editor (Portal)

1. Open database **`free-sql-db-6245011`**.
2. **Query editor** → login with server admin.
3. Run:

```sql
CREATE USER edusync_app WITH PASSWORD = 'YourStrongAppPassword!ChangeMe';
ALTER ROLE db_owner ADD MEMBER edusync_app;
```

### Option B — Azure CLI (from your PC)

```powershell
az sql db query `
  --resource-group ERP-SUITE-RG `
  --server erp-sql-abhishek `
  --database free-sql-db-6245011 `
  --admin-user YOUR_SERVER_ADMIN `
  --admin-password "YOUR_SERVER_ADMIN_PASSWORD" `
  --query-text "CREATE USER edusync_app WITH PASSWORD = 'YourStrongAppPassword!ChangeMe'; ALTER ROLE db_owner ADD MEMBER edusync_app;"
```

---

## Part 3 — Connection string for EduSync

Replace passwords and database name if you changed them:

```text
Server=tcp:erp-sql-abhishek.database.windows.net,1433;
Initial Catalog=free-sql-db-6245011;
User ID=edusync_app;
Password=YourStrongAppPassword!ChangeMe;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
Min Pool Size=5;
Max Pool Size=100;
```

**Do not commit this to git.** Use User Secrets locally:

```powershell
cd C:\Users\rajab\Desktop\school-erp-backend\src\EduSync.Api

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:erp-sql-abhishek.database.windows.net,1433;Initial Catalog=free-sql-db-6245011;User ID=edusync_app;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

## Part 4 — Create tables (EF migrations)

```powershell
cd C:\Users\rajab\Desktop\school-erp-backend

$env:ConnectionStrings__DefaultConnection = "PASTE_SAME_CONNECTION_STRING"

dotnet ef database update `
  --project src/EduSync.Infrastructure `
  --startup-project src/EduSync.Api
```

Success = all schemas (`students`, `fees`, `identity`, …) created.

---

## Part 5 — Run API & seed demo data

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/EduSync.Api
```

Startup runs **SeedData** → demo tenant `demo-school-001`, admin user.

### Test

```powershell
.\scripts\local\smoke-test.ps1 -BaseUrl http://localhost:5000
```

Or Swagger: `POST /api/auth/login` with `admin@school.edu` / `admin123` and header `X-Tenant-Id: demo-school-001`.

---

## Part 6 — Connect Next.js dashboard (optional)

```env
NEXT_PUBLIC_USE_MOCK=false
NEXT_PUBLIC_API_URL=http://localhost:5000/api
API_URL=http://localhost:5000/api
```

(API still runs locally; only **database** is on Azure.)

---

## Free tier limits (important)

| Limit | Free offer |
|-------|------------|
| Compute | 100,000 vCore-seconds / month |
| Data | 32 GB |
| Backup | 32 GB |
| Overage | **Disabled** → DB **pauses** when free quota is used |

If the DB **pauses**, resume in Portal: database → **Resume**, or upgrade to paid tier.

**Pilot / 10 schools production:** move to **General Purpose 2–4 vCore** (paid) — see [AZURE_DATABASE_SETUP.md](AZURE_DATABASE_SETUP.md).

---

## Troubleshooting

| Error | Fix |
|-------|-----|
| Cannot open server … firewall | Add your IP on server **Networking** |
| Login failed | Wrong user/password; create `edusync_app` on **this** database |
| SSL/certificate | Keep `Encrypt=True;TrustServerCertificate=False` |
| Migration timeout | Firewall + stable internet; retry |
| Database paused | Free quota exhausted — Resume or upgrade |

---

## Quick reference

| Item | Value |
|------|--------|
| Server FQDN | `erp-sql-abhishek.database.windows.net` |
| Database | `free-sql-db-6245011` |
| Region | Central India |
| Resource group | `ERP-SUITE-RG` |
