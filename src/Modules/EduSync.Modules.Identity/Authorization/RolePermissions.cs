using EduSync.Modules.Identity.Domain;

namespace EduSync.Modules.Identity.Authorization;

public static class RolePermissions
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Map = BuildMap();

    public static bool HasPermission(string? role, string permission)
    {
        if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        return Map.TryGetValue(role, out var permissions)
            && permissions.Contains(permission);
    }

    public static IReadOnlyList<string> GetPermissionsForRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role) || !Map.TryGetValue(role, out var permissions))
        {
            return [];
        }

        return permissions.OrderBy(p => p, StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyDictionary<string, IReadOnlySet<string>> BuildMap()
    {
        var all = Permissions.All.ToHashSet(StringComparer.Ordinal);
        var principal = all.Except(Permissions.AdminOnly).ToHashSet(StringComparer.Ordinal);

        var teacher = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.StudentsRead,
            Permissions.TeachersRead,
            Permissions.AcademicsRead,
            Permissions.AttendanceRead,
            Permissions.AttendanceWrite,
            Permissions.ExamsRead,
            Permissions.ExamsWrite,
            Permissions.AssignmentsRead,
            Permissions.AssignmentsWrite,
            Permissions.TimetableRead,
            Permissions.LeaveRead,
            Permissions.LeaveWrite,
            Permissions.LibraryRead,
            Permissions.LibraryWrite,
            Permissions.NotificationsRead,
            Permissions.DashboardRead,
            Permissions.ReportsRead,
            Permissions.UploadsRead,
            Permissions.UploadsWrite,
            Permissions.TenantsRead,
            Permissions.PortalTeacher,
        };

        var student = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.TenantsRead,
            Permissions.PortalStudent,
        };

        var parent = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.TenantsRead,
            Permissions.PortalParent,
        };

        var company = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.CompanyRead,
            Permissions.EnquiriesRead,
            Permissions.EnquiriesManage,
            Permissions.TenantsManage,
            Permissions.TenantsRead,
        };

        return new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [UserRoles.Admin] = all,
            [UserRoles.Principal] = principal,
            [UserRoles.Teacher] = teacher,
            [UserRoles.Student] = student,
            [UserRoles.Parent] = parent,
            [UserRoles.Company] = company,
        };
    }
}
