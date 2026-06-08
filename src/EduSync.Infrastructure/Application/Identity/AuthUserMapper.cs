using EduSync.Modules.Identity.Application.Dtos;
using EduSync.Modules.Identity.Authorization;
using EduSync.Modules.Identity.Domain;

namespace EduSync.Infrastructure.Application.Identity;

internal static class AuthUserMapper
{
    public static AuthUserDto ToDto(User user, TenantMembership membership, string tenantExternalId) =>
        new(
            user.ExternalId,
            user.Name,
            user.Email,
            membership.Role,
            tenantExternalId,
            RolePermissions.GetPermissionsForRole(membership.Role));

    public static AuthUserDto ToCompanyDto(User user) =>
        new(
            user.ExternalId,
            user.Name,
            user.Email,
            UserRoles.Company,
            string.Empty,
            RolePermissions.GetPermissionsForRole(UserRoles.Company));
}
