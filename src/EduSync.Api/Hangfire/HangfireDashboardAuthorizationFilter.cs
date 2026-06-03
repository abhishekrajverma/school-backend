using System.Security.Claims;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Authorization;
using Hangfire.Dashboard;

namespace EduSync.Api.Hangfire;

public sealed class HangfireDashboardAuthorizationFilter(IHostEnvironment environment) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        if (environment.IsDevelopment()) return true;

        var http = context.GetHttpContext();
        var permissionService = http.RequestServices.GetRequiredService<IPermissionService>();
        var role = http.User.FindFirstValue(ClaimTypes.Role);
        return permissionService.HasPermission(role, Permissions.JobsRun);
    }
}
