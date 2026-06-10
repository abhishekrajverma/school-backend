using EduSync.SharedKernel.Abstractions;

namespace EduSync.Modules.Tenancy.Domain;

public sealed partial class AcademicYear : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
