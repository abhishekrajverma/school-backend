using Microsoft.AspNetCore.Authorization;

namespace EduSync.Infrastructure.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
