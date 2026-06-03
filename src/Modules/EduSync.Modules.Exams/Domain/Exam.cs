using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Exams.Domain;

public sealed class Exam : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public string ExamType { get; set; } = "mid_term";
    public string Subject { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string StartTime { get; set; } = "09:00";
    public int DurationMinutes { get; set; }
    public int TotalMarks { get; set; }
    public int PassingMarks { get; set; }
    public string? Room { get; set; }
    public string Status { get; set; } = "scheduled";
    public int StudentsCount { get; set; }
}
