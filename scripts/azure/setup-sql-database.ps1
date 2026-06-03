#Requires -Version 5.1
<#
.SYNOPSIS
  Creates Azure SQL Server + Database + firewall + app login for EduSync.
.DESCRIPTION
  Run after: az login
  Copy setup-sql.parameters.example.json to setup-sql.parameters.json and edit passwords.
.EXAMPLE
  .\setup-sql-database.ps1 -ParametersFile .\setup-sql.parameters.json
#>
param(
    [string]$ParametersFile = "$PSScriptRoot\setup-sql.parameters.json"
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI (az) is not installed. Install from https://aka.ms/installazurecliwindows"
}

if (-not (Test-Path $ParametersFile)) {
    Write-Error "Missing $ParametersFile — copy setup-sql.parameters.example.json and edit passwords."
}

$p = Get-Content $ParametersFile -Raw | ConvertFrom-Json

foreach ($required in @("sqlServerName", "sqlAdminPassword", "appUserPassword")) {
    if ($p.$required -match "CHANGE_ME") {
        Write-Error "Edit $ParametersFile — set a real password for $required before running."
    }
}

$rg = $p.resourceGroup
$location = $p.location
$server = $p.sqlServerName
$db = $p.databaseName
$adminUser = $p.sqlAdminUser
$adminPass = $p.sqlAdminPassword
$appUser = $p.appUserName
$appPass = $p.appUserPassword
$vCores = [int]($p.vCores)
$maxSizeGb = [int]($p.maxSizeGb)

Write-Host "Using subscription:" -ForegroundColor Cyan
az account show --query "{name:name, id:id}" -o table

Write-Host "Creating resource group $rg in $location..." -ForegroundColor Cyan
az group create --name $rg --location $location --output none

Write-Host "Creating SQL server $server..." -ForegroundColor Cyan
az sql server create `
    --name $server `
    --resource-group $rg `
    --location $location `
    --admin-user $adminUser `
    --admin-password $adminPass `
    --enable-public-network true `
    --output none

if ($p.allowAzureServices) {
    Write-Host "Firewall: Allow Azure services..." -ForegroundColor Cyan
    az sql server firewall-rule create `
        --resource-group $rg `
        --server $server `
        --name AllowAzureServices `
        --start-ip-address 0.0.0.0 `
        --end-ip-address 0.0.0.0 `
        --output none
}

if ($p.addCurrentClientIpFirewallRule) {
    Write-Host "Firewall: Adding your current public IP..." -ForegroundColor Cyan
    $ip = (Invoke-RestMethod -Uri "https://api.ipify.org" -TimeoutSec 15).Trim()
    az sql server firewall-rule create `
        --resource-group $rg `
        --server $server `
        --name "AllowClient_$($ip -replace '\.', '_')" `
        --start-ip-address $ip `
        --end-ip-address $ip `
        --output none
    Write-Host "  Allowed IP: $ip"
}

Write-Host "Creating database $db ($vCores vCore GP)..." -ForegroundColor Cyan
az sql db create `
    --resource-group $rg `
    --server $server `
    --name $db `
    --edition GeneralPurpose `
    --compute-model Provisioned `
    --capacity $vCores `
    --max-size "${maxSizeGb}GB" `
    --backup-storage-redundancy Local `
    --output none

Write-Host "Creating application SQL user $appUser on database..." -ForegroundColor Cyan
$escapedAppPass = $appPass.Replace("'", "''")
$sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$appUser')
    CREATE USER [$appUser] WITH PASSWORD = N'$escapedAppPass';
ELSE
    ALTER USER [$appUser] WITH PASSWORD = N'$escapedAppPass';
IF IS_ROLEMEMBER('db_owner', '$appUser') = 0
    ALTER ROLE db_owner ADD MEMBER [$appUser];
"@

$sql | az sql db query `
    --resource-group $rg `
    --server $server `
    --database $db `
    --admin-user $adminUser `
    --admin-password $adminPass `
    --output none

$fqdn = "$server.database.windows.net"
$connectionString = @"
Server=tcp:$fqdn,1433;Initial Catalog=$db;User ID=$appUser;Password=$appPass;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Min Pool Size=10;Max Pool Size=200;
"@ -replace "`r`n", ""

Write-Host ""
Write-Host "========== SUCCESS ==========" -ForegroundColor Green
Write-Host "Resource group : $rg"
Write-Host "SQL server     : $fqdn"
Write-Host "Database       : $db"
Write-Host ""
Write-Host "Connection string (store in App Service / User Secrets — do NOT commit):" -ForegroundColor Yellow
Write-Host $connectionString
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Set ConnectionStrings__DefaultConnection to the string above"
Write-Host "  2. dotnet ef database update --project src/EduSync.Infrastructure --startup-project src/EduSync.Api"
Write-Host "  3. dotnet run --project src/EduSync.Api (seeds demo-school-001)"
Write-Host "  See docs/AZURE_DATABASE_SETUP.md"
Write-Host "=============================" -ForegroundColor Green

# Save to local gitignored file for convenience
$outFile = Join-Path $PSScriptRoot "connection-string.local.txt"
$connectionString | Set-Content -Path $outFile -Encoding UTF8
Write-Host "Also saved to: $outFile (add to .gitignore if not already)" -ForegroundColor DarkGray
