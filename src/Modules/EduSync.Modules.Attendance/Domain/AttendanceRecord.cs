using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Attendance.Domain;

public sealed class AttendanceRecord : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public string EntityType { get; set; } = "student";
    public string EntityExternalId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public DateOnly Date { get; set; }
    public string Status { get; set; } = "present";
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public string? Remarks { get; set; }
}
