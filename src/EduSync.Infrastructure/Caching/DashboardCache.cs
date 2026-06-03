using System.Text.Json;
using EduSync.Modules.Dashboard.Application;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Caching;

public sealed class DashboardCache(IDistributedCache cache, IOptions<RedisOptions> redisOptions) : IDashboardCache
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly RedisOptions _options = redisOptions.Value;

    public async Task<DashboardResponseDto?> GetAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var json = await cache.GetStringAsync($"dashboard:{tenantId:N}", cancellationToken);
        return json is null ? null : JsonSerializer.Deserialize<DashboardResponseDto>(json, JsonOptions);
    }

    public async Task SetAsync(Guid tenantId, DashboardResponseDto dto, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await cache.SetStringAsync(
            $"dashboard:{tenantId:N}",
            JsonSerializer.Serialize(dto, JsonOptions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.DashboardCacheSeconds),
            },
            cancellationToken);
    }
}
