using System.Security.Claims;
using EduSync.Modules.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace EduSync.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler(
    IPermissionService permissionService,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var role = httpContextAccessor.HttpContext?.Items["tenant_role"] as string
            ?? context.User.FindFirstValue(ClaimTypes.Role);
        if (permissionService.HasPermission(role, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
