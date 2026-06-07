using EduSync.Infrastructure.Caching;
using EduSync.Infrastructure.Tenancy;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EduSync.Infrastructure.Middleware;

public sealed class TenantRateLimitMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/api/health",
        "/swagger",
        "/hangfire",
        "/hubs",
    };

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        IOptions<RedisOptions> redisOptions)
    {
        var options = redisOptions.Value;
        var redis = context.RequestServices.GetService<IConnectionMultiplexer>();
        if (!options.Enabled || redis is null || options.RateLimitPerMinute <= 0)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var tenantKey = context.Request.Headers[HttpHeaders.TenantId].FirstOrDefault()
            ?? tenantContext.TenantExternalId
            ?? "anonymous";
        var minute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        var key = $"ratelimit:{tenantKey}:{minute}";
        var db = redis.GetDatabase();
        var count = await db.StringIncrementAsync(key);
        if (count == 1)
        {
            await db.KeyExpireAsync(key, TimeSpan.FromMinutes(2));
        }

        if (count > options.RateLimitPerMinute)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = "60";
            await context.Response.WriteAsJsonAsync(new
            {
                code = "RATE_LIMITED",
                message = "Too many requests for this tenant. Try again shortly.",
            });
            return;
        }

        await next(context);
    }
}
