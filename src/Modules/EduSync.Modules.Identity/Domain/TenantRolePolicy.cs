namespace EduSync.Modules.Identity.Domain;

public static class TenantRolePolicy
{
    private static readonly HashSet<string> TenantWideRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        UserRoles.Admin,
        UserRoles.Principal,
    };

    public static bool HasTenantWideBranchAccess(string? tenantRole) =>
        !string.IsNullOrWhiteSpace(tenantRole) && TenantWideRoles.Contains(tenantRole);
}
