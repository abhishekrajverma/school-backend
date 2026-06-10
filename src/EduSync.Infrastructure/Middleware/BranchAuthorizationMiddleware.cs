using System.Security.Claims;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Identity.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Middleware;

/// <summary>
/// Enforces branch-level access for authenticated users when a branch context is resolved.
/// Admin and principal roles have tenant-wide branch access.
/// </summary>
public sealed class BranchAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IBranchContext branchContext,
        IRequestBranchRoleContext branchRole,
        EduSyncDbContext db)
    {
        if (!branchContext.IsResolved
            || context.User.Identity?.IsAuthenticated != true
            || !context.Items.TryGetValue("tenant_role", out var tenantRoleObj))
        {
            await next(context);
            return;
        }

        var tenantRole = tenantRoleObj as string;
        if (TenantRolePolicy.HasTenantWideBranchAccess(tenantRole))
        {
            branchRole.Set(tenantRole!);
            context.Items["branch_role"] = tenantRole;
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

        var membership = await db.BranchMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.BranchId == branchContext.BranchId
                     && m.UserId == userId
                     && m.IsActive,
                context.RequestAborted);

        if (membership is null)
        {
            branchRole.Deny();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { code = "FORBIDDEN", message = "User does not have access to this branch." });
            return;
        }

        branchRole.Set(membership.Role);
        context.Items["branch_role"] = membership.Role;
        await next(context);
    }
}
