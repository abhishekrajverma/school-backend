using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Middleware;

public sealed class BranchResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        IBranchContext branchContext,
        EduSyncDbContext db)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            await next(context);
            return;
        }

        var branchHeader = context.Request.Headers[HttpHeaders.BranchId].FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(branchHeader))
        {
            var defaultBranch = await db.Branches
                .AsNoTracking()
                .Where(b => b.TenantId == tenantContext.TenantId && b.IsActive)
                .OrderByDescending(b => b.IsHeadOffice)
                .ThenBy(b => b.CreatedAt)
                .FirstOrDefaultAsync(context.RequestAborted);

            if (defaultBranch is not null)
            {
                branchContext.Set(defaultBranch.Id, defaultBranch.ExternalId);
            }

            await next(context);
            return;
        }

        var branch = Guid.TryParse(branchHeader, out var branchGuid)
            ? await db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(
                    b => b.TenantId == tenantContext.TenantId && b.Id == branchGuid && b.IsActive,
                    context.RequestAborted)
            : await db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(
                    b => b.TenantId == tenantContext.TenantId
                         && (b.ExternalId == branchHeader || b.Code == branchHeader)
                         && b.IsActive,
                    context.RequestAborted);

        if (branch is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { code = "FORBIDDEN", message = "Branch not found." });
            return;
        }

        branchContext.Set(branch.Id, branch.ExternalId);
        await next(context);
    }
}
