using EduSync.SharedKernel.Abstractions;

namespace EduSync.SharedKernel.Entities;

public abstract class BranchEntity : TenantEntity, IBranchEntity
{
    public Guid BranchId { get; set; }
}
