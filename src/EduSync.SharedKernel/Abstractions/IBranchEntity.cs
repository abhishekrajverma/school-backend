namespace EduSync.SharedKernel.Abstractions;

public interface IBranchEntity : ITenantEntity
{
    Guid BranchId { get; }
}
