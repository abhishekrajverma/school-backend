using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Assignments.Domain;

public sealed class Assignment : BranchEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid AcademicYearId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Subject { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = AssignmentStatuses.Published;
    public string? TeacherExternalId { get; set; }
}

public static class AssignmentStatuses
{
    public const string Draft = "draft";
    public const string Published = "published";
    public const string Closed = "closed";
}
