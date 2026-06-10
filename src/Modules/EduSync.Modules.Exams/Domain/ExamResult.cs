using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Exams.Domain;

public sealed class ExamResult : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid AcademicYearId { get; set; }
    public Guid? BranchId { get; set; }
    public string ExamExternalId { get; set; } = string.Empty;
    public string StudentExternalId { get; set; } = string.Empty;
    public decimal MarksObtained { get; set; }
    public decimal TotalMarks { get; set; }
    public string? Grade { get; set; }
    public string Status { get; set; } = ExamResultStatuses.Published;
    public string? Remarks { get; set; }
}

public static class ExamResultStatuses
{
    public const string Draft = "draft";
    public const string Published = "published";
}
