using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Students.Domain;

public sealed class StudentParent : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }
    public string Relationship { get; set; } = "guardian";
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public Student Student { get; set; } = null!;
}
