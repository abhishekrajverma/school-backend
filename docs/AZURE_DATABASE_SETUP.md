# Azure SQL Database setup for EduSync

Use this guide to create **one Azure SQL Database** for EduSync (10 schools to start, scale later).  
I cannot access your Azure account — you run the steps below in **Azure Portal** or the provided script on your PC.

---

## What you will create

| Resource | Starter (10 schools) | Name example |
|----------|----------------------|--------------|
| Resource group | `edusync-prod-rg` | |
| SQL server | Logical server + TLS | `edusync-sql-prod-001` |
| SQL database | **General Purpose, 2–4 vCore** | `EduSync` |
| SQL login | App user (not server admin in prod) | `edusync_app` |
| Firewall | Azure services + your IP | |

---

## Option A — PowerShell script (recommended)

### Prerequisites

1. [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows) installed.
2. Login: `az login`
3. Set subscription (if you have several):

```powershell
az account list -o table
az account set --subscription "YOUR_SUBSCRIPTION_NAME_OR_ID"
```

### Run the script

From the repo root:

```powershell
cd C:\Users\rajab\Desktop\school-erp-backend\scripts\azure

# Copy and edit parameters first
Copy-Item setup-sql.parameters.example.json setup-sql.parameters.json
# Edit setup-sql.parameters.json — set sqlAdminPassword and appUserPassword

.\setup-sql-database.ps1 -ParametersFile .\setup-sql.parameters.json
```

The script prints a **connection string** at the end. Paste it into:

- `src/EduSync.Api/appsettings.Production.json` → `ConnectionStrings:DefaultConnection`
- Or Azure App Service → **Configuration** → Connection strings (name: `DefaultConnection`, type: SQLAzure)

### Apply EF migrations to Azure

```powershell
cd C:\Users\rajab\Desktop\school-erp-backend

$env:ConnectionStrings__DefaultConnection = "PASTE_FULL_CONNECTION_STRING_HERE"

dotnet ef database update `
  --project src/EduSync.Infrastructure `
  --startup-project src/EduSync.Api
```

Start the API once so **SeedData** runs (demo tenant `demo-school-001`):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project src/EduSync.Api
```

---

## Option B — Azure Portal (click-through)

### 1. Resource group

1. [Azure Portal](https://portal.azure.com) → **Resource groups** → **Create**.
2. Name: `edusync-prod-rg`, Region: **Central India** (or South India).

### 2. SQL server

1. **Create a resource** → **SQL server**.
2. **Server name:** globally unique, e.g. `edusync-sql-prod-001`.
3. **Authentication:** SQL authentication.
4. **Server admin login:** e.g. `sqladmin` (save password in Key Vault).
5. **Networking:** Allow Azure services (you can refine later).
6. Create.

### 3. SQL database

1. Open the server → **SQL databases** → **Create**.
2. **Database name:** `EduSync`.
3. **Workload environment:** Production.
4. **Compute + storage:** **General Purpose** → **Provisioned**.
5. For **10 schools:** start with **2 vCore** (scale to 4 when CPU &gt; 70%).
6. **Backup:** Geo-redundant optional (costs more).
7. Create.

### 4. Firewall

On the SQL **server** (not only the database):

1. **Networking** → **Firewall rules**.
2. **Add your client IPv4** (for migrations from your PC).
3. Enable **Allow Azure services and resources to access this server** (needed for App Service).

### 5. App SQL user (recommended)

Connect with **Query editor** (Azure AD or sqladmin) or SSMS:

```sql
CREATE USER edusync_app WITH PASSWORD = 'YourStrongAppPassword!ChangeMe';
ALTER ROLE db_owner ADD MEMBER edusync_app;
```

For stricter production, use `db_datareader` + `db_datawriter` + `ddladmin` only if your deployment pipeline uses a separate migration login.

### 6. Connection string

Format for .NET:

```text
Server=tcp:edusync-sql-prod-001.database.windows.net,1433;
Initial Catalog=EduSync;
User ID=edusync_app;
Password=YOUR_APP_PASSWORD;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
Min Pool Size=10;
Max Pool Size=200;
```

Put this in `appsettings.Production.json` or App Service configuration.

---

## App Service / local configuration

| Setting | Value |
|---------|--------|
| `Capacity:SingleDatabase` | `true` |
| `Database:UseReadReplica` | `false` |
| `ConnectionStrings:DefaultConnection` | Azure string above |

**Never commit real passwords.** Use:

- **Azure App Service** → Application settings (slot settings for prod).
- **User Secrets** for local dev against Azure:

```powershell
cd src/EduSync.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:..."
```

---

## Verify database

```powershell
# Health
curl https://YOUR-API/api/health

# Login (after seed)
curl -X POST https://YOUR-API/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"email":"admin@school.edu","password":"admin123"}'
```

Use header `X-Tenant-Id: demo-school-001` on API calls.

---

## Scale the database later

| Schools | Azure SQL change |
|---------|------------------|
| 10 → 25 | 2 → **4 vCore**, storage +128 GB |
| 25 → 100 | **6 vCore** |
| 100 → 200 | **8 vCore**, review indexes & pool size |

Portal: database → **Compute + storage** → **Scale**.

---

## Troubleshooting

| Error | Fix |
|-------|-----|
| Cannot open server ... firewall | Add your IP; enable Azure services rule |
| Login failed for user | Check `User ID` / password; user exists on `EduSync` DB |
| Certificate / SSL | Use `Encrypt=True;TrustServerCertificate=False` on Azure |
| Migration timeout | Temporarily allow your IP; increase `CommandTimeoutSeconds` |
| Too many connections | Lower `Max Pool Size` per instance × number of API instances |

---

## Related

- [CAPACITY.md](CAPACITY.md) — phased Azure sizing (10 vs 200 schools)
- [CODEBASE_GUIDE.md](CODEBASE_GUIDE.md) — code layout
- `scripts/azure/setup-sql-database.ps1` — automated provisioning
