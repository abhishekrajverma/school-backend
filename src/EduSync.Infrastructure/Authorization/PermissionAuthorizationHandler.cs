using System.Security.Claims;
using EduSync.Modules.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace EduSync.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler(IPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var role = context.User.FindFirstValue(ClaimTypes.Role);
        if (permissionService.HasPermission(role, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
