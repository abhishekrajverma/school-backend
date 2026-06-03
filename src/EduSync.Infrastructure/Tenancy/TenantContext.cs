namespace EduSync.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public string? TenantExternalId { get; private set; }
    public bool IsResolved => TenantId.HasValue;

    public void Set(Guid tenantId, string slug, string? externalId)
    {
        TenantId = tenantId;
        TenantSlug = slug;
        TenantExternalId = externalId;
    }
}
