namespace EduSync.Infrastructure.Tenancy;

public sealed class RequestTenantRoleContext : IRequestTenantRoleContext
{
    public string? Role { get; private set; }

    public void Set(string role) => Role = role;
}
