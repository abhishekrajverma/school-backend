namespace EduSync.Modules.Identity.Application.Abstractions;

public interface IPermissionService
{
    bool HasPermission(string? role, string permission);

    IReadOnlyList<string> GetPermissionsForRole(string? role);
}
