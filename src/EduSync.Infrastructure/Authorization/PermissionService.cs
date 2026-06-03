using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Authorization;

namespace EduSync.Infrastructure.Authorization;

public sealed class PermissionService : IPermissionService
{
    public bool HasPermission(string? role, string permission) =>
        RolePermissions.HasPermission(role, permission);

    public IReadOnlyList<string> GetPermissionsForRole(string? role) =>
        RolePermissions.GetPermissionsForRole(role);
}
