using EduSync.Modules.Tenancy.Domain;

namespace EduSync.Infrastructure.Caching;

public interface ITenantLookupCache
{
    Task<Tenant?> GetByKeyAsync(string tenantKey, CancellationToken cancellationToken = default);
    void Invalidate(string tenantKey);
}
