using System.Security.Claims;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Middleware;

/// <summary>
/// Binds JWT identity to the resolved tenant: validates tenant_id claim and resolves role from membership.
/// </summary>
public sealed class TenantAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        IRequestTenantRoleContext requestRole,
        EduSyncDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated != true || !tenantContext.IsResolved)
        {
            await next(context);
            return;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            await next(context);
            return;
        }

        var jwtTenantId = context.User.FindFirstValue("tenant_id");
        if (jwtTenantId is not null
            && Guid.TryParse(jwtTenantId, out var claimTenantId)
            && claimTenantId != Guid.Empty
            && claimTenantId != tenantContext.TenantId)
        {
            await WriteForbiddenAsync(context, "JWT tenant does not match request tenant.");
            return;
        }

        var membership = await db.TenantMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.TenantId == tenantContext.TenantId && m.UserId == userId && m.IsActive,
                context.RequestAborted);

        if (membership is null)
        {
            await WriteForbiddenAsync(context, "User is not a member of this tenant.");
            return;
        }

        requestRole.Set(membership.Role);
        context.Items["tenant_role"] = membership.Role;
        await next(context);
    }

    private static async Task WriteForbiddenAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { code = "FORBIDDEN", message });
    }
}
