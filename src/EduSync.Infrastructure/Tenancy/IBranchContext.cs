namespace EduSync.Infrastructure.Tenancy;

public interface IBranchContext
{
    Guid? BranchId { get; }
    string? BranchExternalId { get; }
    bool IsResolved => BranchId.HasValue;
    void Set(Guid branchId, string? externalId);
}
