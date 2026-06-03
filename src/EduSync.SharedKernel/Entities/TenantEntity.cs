using EduSync.SharedKernel.Abstractions;

namespace EduSync.SharedKernel.Entities;

public abstract class TenantEntity : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public bool IsDeleted { get; set; }
}
