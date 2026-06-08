using System.Text.Json;
using EduSync.Modules.Dashboard.Application;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Caching;

public sealed class DashboardCache(IDistributedCache cache, IOptions<RedisOptions> redisOptions) : IDashboardCache
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly RedisOptions _options = redisOptions.Value;

    public async Task<DashboardResponseDto?> GetAsync(Guid tenantId, string financialYear, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var json = await cache.GetStringAsync(CacheKey(tenantId, financialYear), cancellationToken);
        return json is null ? null : JsonSerializer.Deserialize<DashboardResponseDto>(json, JsonOptions);
    }

    public async Task SetAsync(Guid tenantId, string financialYear, DashboardResponseDto dto, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await cache.SetStringAsync(
            CacheKey(tenantId, financialYear),
            JsonSerializer.Serialize(dto, JsonOptions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.DashboardCacheSeconds),
            },
            cancellationToken);
    }

    private static string CacheKey(Guid tenantId, string financialYear) =>
        $"dashboard:{tenantId:N}:{financialYear}";
}
