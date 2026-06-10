namespace EduSync.Infrastructure.Tenancy;

public sealed class RequestBranchRoleContext : IRequestBranchRoleContext
{
    public string? Role { get; private set; }
    public bool HasBranchAccess { get; private set; }

    public void Set(string role)
    {
        Role = role;
        HasBranchAccess = true;
    }

    public void Deny() => HasBranchAccess = false;
}
