namespace EduSync.Infrastructure.Tenancy;

public interface IRequestBranchRoleContext
{
    string? Role { get; }
    bool HasBranchAccess { get; }
    void Set(string role);
    void Deny();
}
