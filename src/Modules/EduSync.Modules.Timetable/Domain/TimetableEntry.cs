using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Timetable.Domain;

public sealed class TimetableEntry : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Day { get; set; } = string.Empty;
    public string PeriodsJson { get; set; } = "[]";
}
