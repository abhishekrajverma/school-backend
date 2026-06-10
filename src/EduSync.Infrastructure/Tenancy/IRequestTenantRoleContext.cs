namespace EduSync.Infrastructure.Tenancy;

/// <summary>
/// Role resolved from TenantMembership for the current request tenant (not JWT claim alone).
/// </summary>
public interface IRequestTenantRoleContext
{
    string? Role { get; }
    void Set(string role);
}
