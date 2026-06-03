using System.Text.Json;
using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Caching;

public sealed class TenantLookupCache(
    EduSyncDbContext db,
    IDistributedCache cache,
    IOptions<RedisOptions> redisOptions) : ITenantLookupCache
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly RedisOptions _options = redisOptions.Value;

    public async Task<Tenant?> GetByKeyAsync(string tenantKey, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"tenant:lookup:{tenantKey}";
        if (_options.Enabled)
        {
            var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return JsonSerializer.Deserialize<TenantSnapshot>(cached, JsonOptions)?.ToEntity();
            }
        }

        var tenant = await ResolveFromDbAsync(tenantKey, cancellationToken);
        if (tenant is not null && _options.Enabled)
        {
            var snapshot = TenantSnapshot.FromEntity(tenant);
            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(snapshot, JsonOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.TenantCacheMinutes),
                },
                cancellationToken);
        }

        return tenant;
    }

    public void Invalidate(string tenantKey) =>
        cache.Remove($"tenant:lookup:{tenantKey}");

    private async Task<Tenant?> ResolveFromDbAsync(string tenantKey, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(tenantKey, out var tenantId))
        {
            return await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        }

        var byExternal = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalId == tenantKey, cancellationToken);
        if (byExternal is not null)
        {
            return byExternal;
        }

        return await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == tenantKey, cancellationToken);
    }

    private sealed record TenantSnapshot(
        Guid Id,
        string ExternalId,
        string Slug,
        string Name,
        string? LogoUrl,
        TenantStatus Status,
        DateTime CreatedAt)
    {
        public static TenantSnapshot FromEntity(Tenant t) =>
            new(t.Id, t.ExternalId, t.Slug, t.Name, t.LogoUrl, t.Status, t.CreatedAt);

        public Tenant ToEntity() => new()
        {
            Id = Id,
            ExternalId = ExternalId,
            Slug = Slug,
            Name = Name,
            LogoUrl = LogoUrl,
            Status = Status,
            CreatedAt = CreatedAt,
        };
    }
}
