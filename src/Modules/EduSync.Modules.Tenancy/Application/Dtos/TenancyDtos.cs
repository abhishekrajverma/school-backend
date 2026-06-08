namespace EduSync.Modules.Tenancy.Application.Dtos;

public sealed record ProvisionTenantRequest(
    string SchoolName,
    string Slug,
    string AdminEmail,
    string AdminPassword,
    string AdminName,
    string PlanId);

public sealed record ProvisionTenantResponse(
    string TenantId,
    string Slug,
    string PortalUrl);

public sealed record TenantBrandingDto(
    string Id,
    string Slug,
    string Name,
    string? LogoUrl,
    string Status);

public sealed record CurrentTenantDto(
    string Id,
    string Slug,
    string Name,
    string? SchoolEmail,
    string? LogoUrl,
    string Status,
    string PlanKey,
    int SeatLimit,
    DateTime? ExpiresAt);
