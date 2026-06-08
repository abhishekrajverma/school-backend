using System.Security.Claims;
using EduSync.Infrastructure.Caching;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSync.Infrastructure.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> TenantOptionalPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/health",
        "/api/auth/login",
        "/api/auth/refresh",
        "/api/tenants/provision",
        "/api/enquiries",
        "/api/company",
        "/swagger",
        "/openapi",
        "/hangfire",
        "/hubs",
    };

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        EduSyncDbContext db,
        ITenantLookupCache tenantCache,
        ILogger<TenantResolutionMiddleware> logger)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (IsTenantOptional(path))
        {
            await TrySetTenantFromHeaderAsync(context, tenantCache, tenantContext, context.RequestAborted);
            await next(context);
            return;
        }

        var tenantHeader = context.Request.Headers[HttpHeaders.TenantId].FirstOrDefault();
        var slugHeader = context.Request.Headers[HttpHeaders.TenantSlug].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantHeader) && path.StartsWith("/api/tenants/by-slug/", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(tenantHeader))
        {
            await WriteForbiddenAsync(context, "Tenant context is required.");
            return;
        }

        var tenant = await ResolveTenantAsync(tenantCache, tenantHeader.Trim(), slugHeader, context.RequestAborted);
        if (tenant is null)
        {
            await WriteForbiddenAsync(context, "Tenant not found.");
            return;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            var message = tenant.Status == TenantStatus.Suspended
                ? "School suspended."
                : "School not active yet.";
            await WriteForbiddenAsync(context, message);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");
            if (userIdClaim is not null && Guid.TryParse(userIdClaim, out var userId))
            {
                var isMember = await db.TenantMemberships
                    .AsNoTracking()
                    .AnyAsync(
                        m => m.TenantId == tenant.Id && m.UserId == userId && m.IsActive,
                        context.RequestAborted);

                if (!isMember)
                {
                    await WriteForbiddenAsync(context, "User is not a member of this tenant.");
                    return;
                }
            }
        }

        tenantContext.Set(tenant.Id, tenant.Slug, tenant.ExternalId);
        await next(context);
    }

    private static async Task TrySetTenantFromHeaderAsync(
        HttpContext context,
        ITenantLookupCache tenantCache,
        ITenantContext tenantContext,
        CancellationToken cancellationToken)
    {
        var tenantHeader = context.Request.Headers[HttpHeaders.TenantId].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tenantHeader))
        {
            return;
        }

        var tenant = await ResolveTenantAsync(tenantCache, tenantHeader.Trim(), null, cancellationToken);
        if (tenant is not null && tenant.Status == TenantStatus.Active)
        {
            tenantContext.Set(tenant.Id, tenant.Slug, tenant.ExternalId);
        }
    }

    private static bool IsTenantOptional(string path)
    {
        if (TenantOptionalPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return path.StartsWith("/api/tenants/by-slug/", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Tenant?> ResolveTenantAsync(
        ITenantLookupCache tenantCache,
        string tenantKey,
        string? slugHeader,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantCache.GetByKeyAsync(tenantKey, cancellationToken);
        if (tenant is not null || string.IsNullOrWhiteSpace(slugHeader))
        {
            return tenant;
        }

        return await tenantCache.GetByKeyAsync(slugHeader, cancellationToken);
    }

    private static async Task WriteForbiddenAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { code = "FORBIDDEN", message });
    }
}
