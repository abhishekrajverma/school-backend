#Requires -Version 5.1
<#
.SYNOPSIS
  Smoke test EduSync API locally (login, RBAC, tenant header).
.EXAMPLE
  .\smoke-test.ps1
  .\smoke-test.ps1 -BaseUrl http://localhost:5000
#>
param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$TenantId = "demo-school-001"
)

$ErrorActionPreference = "Stop"

function Test-Api {
    param([string]$Name, [scriptblock]$Block)
    try {
        & $Block
        Write-Host "[PASS] $Name" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "[FAIL] $Name" -ForegroundColor Red
        Write-Host "       $($_.Exception.Message)" -ForegroundColor DarkRed
        return $false
    }
}

$passed = 0
$failed = 0

Write-Host "EduSync smoke test -> $BaseUrl" -ForegroundColor Cyan
Write-Host "Tenant: $TenantId`n"

if (Test-Api "Health check" {
        $r = Invoke-RestMethod -Uri "$BaseUrl/api/health" -Method Get
        if (-not $r) { throw "Empty health response" }
    }) { $passed++ } else { $failed++ }

$loginBody = @{ email = "admin@school.edu"; password = "admin123" } | ConvertTo-Json
$adminToken = $null

if (Test-Api "Admin login" {
        $login = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
        if (-not $login.accessToken) { throw "No accessToken in login response" }
        $script:adminToken = $login.accessToken
        if (-not $login.user.permissions) { throw "No permissions[] on user (RBAC)" }
    }) { $passed++ } else { $failed++ }

if ($adminToken) {
    $headers = @{
        Authorization = "Bearer $adminToken"
        "X-Tenant-Id"   = $TenantId
    }

    if (Test-Api "Admin list students" {
            $students = Invoke-RestMethod -Uri "$BaseUrl/api/students?page=1&pageSize=5" -Method Get -Headers $headers
            if ($null -eq $students.items) { throw "No items in student list" }
        }) { $passed++ } else { $failed++ }

    if (Test-Api "GET /api/auth/me" {
            $me = Invoke-RestMethod -Uri "$BaseUrl/api/auth/me" -Method Get -Headers @{ Authorization = "Bearer $adminToken" }
            if ($me.role -ne "admin") { throw "Expected role admin, got $($me.role)" }
        }) { $passed++ } else { $failed++ }
}

$studentBody = @{ email = "arjun.s@school.edu"; password = "student123" } | ConvertTo-Json
$studentToken = $null

if (Test-Api "Student login" {
        $login = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $studentBody -ContentType "application/json"
        $script:studentToken = $login.accessToken
    }) { $passed++ } else { $failed++ }

if ($studentToken) {
    $studentHeaders = @{
        Authorization = "Bearer $studentToken"
        "X-Tenant-Id"   = $TenantId
    }

    if (Test-Api "Student blocked from admin students list (403)" {
            try {
                Invoke-RestMethod -Uri "$BaseUrl/api/students?page=1&pageSize=5" -Method Get -Headers $studentHeaders
                throw "Expected 403 Forbidden"
            }
            catch [System.Net.WebException] {
                $resp = $_.Exception.Response
                if ($resp.StatusCode.value__ -ne 403) {
                    throw "Expected 403, got $($resp.StatusCode.value__)"
                }
            }
        }) { $passed++ } else { $failed++ }

    if (Test-Api "Student portal profile" {
            $profile = Invoke-RestMethod -Uri "$BaseUrl/api/students/me" -Method Get -Headers $studentHeaders
            if (-not $profile) { throw "Empty portal profile" }
        }) { $passed++ } else { $failed++ }
}

Write-Host ""
Write-Host "Results: $passed passed, $failed failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Yellow" })
if ($failed -gt 0) { exit 1 }
