namespace EduSync.Infrastructure.Tenancy;

public sealed class BranchContext : IBranchContext
{
    public Guid? BranchId { get; private set; }
    public string? BranchExternalId { get; private set; }

    public void Set(Guid branchId, string? externalId)
    {
        BranchId = branchId;
        BranchExternalId = externalId;
    }
}
