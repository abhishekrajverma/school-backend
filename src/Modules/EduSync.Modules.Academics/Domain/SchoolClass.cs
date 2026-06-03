using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Academics.Domain;

public sealed class SchoolClass : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SectionsJson { get; set; } = "[]";
    public int TotalStudents { get; set; }
    public string? ClassTeacherName { get; set; }
}
