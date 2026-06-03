namespace EduSync.Infrastructure.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    string? TenantExternalId { get; }
    bool IsResolved { get; }
    void Set(Guid tenantId, string slug, string? externalId);
}
