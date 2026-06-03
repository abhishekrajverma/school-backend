using EduSync.Modules.Identity.Authorization;
using EduSync.Modules.Identity.Domain;

namespace EduSync.UnitTests;

public sealed class RolePermissionsTests
{
    [Theory]
    [InlineData(UserRoles.Admin, Permissions.StudentsWrite, true)]
    [InlineData(UserRoles.Principal, Permissions.StudentsWrite, true)]
    [InlineData(UserRoles.Teacher, Permissions.StudentsWrite, false)]
    [InlineData(UserRoles.Student, Permissions.StudentsRead, false)]
    [InlineData(UserRoles.Parent, Permissions.PortalParent, true)]
    [InlineData(UserRoles.Student, Permissions.PortalStudent, true)]
    [InlineData(UserRoles.Principal, Permissions.WebhooksManage, false)]
    [InlineData(UserRoles.Admin, Permissions.WebhooksManage, true)]
    public void HasPermission_maps_roles_correctly(string role, string permission, bool expected) =>
        Assert.Equal(expected, RolePermissions.HasPermission(role, permission));

    [Fact]
    public void Admin_has_all_registered_permissions()
    {
        foreach (var permission in Permissions.All)
        {
            Assert.True(RolePermissions.HasPermission(UserRoles.Admin, permission));
        }
    }
}
