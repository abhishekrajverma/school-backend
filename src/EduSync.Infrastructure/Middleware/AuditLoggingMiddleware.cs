using System.Security.Claims;
using EduSync.Infrastructure.Audit;
using EduSync.Infrastructure.MultiRegion;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Audit.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Middleware;

public sealed class AuditLoggingMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/health",
        "/swagger",
        "/hangfire",
        "/hubs",
        "/gateway/health",
    };

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenant,
        ICurrentUserContext user,
        IRegionContext region,
        IOptions<AuditOptions> options)
    {
        await next(context);

        var auditOpts = options.Value;
        if (!auditOpts.Enabled || !tenant.TenantId.HasValue)
        {
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        if (HttpMethods.IsGet(context.Request.Method) && !auditOpts.LogGetRequests)
        {
            return;
        }

        var action = $"{context.Request.Method.ToLowerInvariant()}.{path.Trim('/').Replace('/', '.')}";
        if (action.Length > 64)
        {
            action = action[..64];
        }

        try
        {
            await using var scope = context.RequestServices.CreateAsyncScope();
            if (scope.ServiceProvider.GetRequiredService<ITenantContext>() is TenantContext scopedTenant)
            {
                scopedTenant.Set(tenant.TenantId.Value, tenant.TenantSlug ?? "", tenant.TenantExternalId);
            }

            var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
            db.AuditLogEntries.Add(new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                Action = action,
                Method = context.Request.Method,
                Path = path.Length > 512 ? path[..512] : path,
                StatusCode = context.Response.StatusCode,
                UserId = user.UserId,
                UserEmail = context.User.FindFirstValue(ClaimTypes.Email),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Region = region.CurrentRegion,
                OccurredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync(context.RequestAborted);
        }
        catch
        {
            // Audit must not break the response pipeline.
        }
    }
}
