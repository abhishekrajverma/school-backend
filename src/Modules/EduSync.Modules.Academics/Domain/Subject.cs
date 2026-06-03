using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Academics.Domain;

public sealed class Subject : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? TeacherExternalId { get; set; }
    public string? TeacherName { get; set; }
    public int WeeklyHours { get; set; }
    public string Status { get; set; } = "active";
}
