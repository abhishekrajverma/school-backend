using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Staff.Domain;

public sealed class TeacherAssignment : BranchEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public Guid AcademicYearId { get; set; }
    public string? ClassName { get; set; }
    public string? SubjectExternalId { get; set; }
    public string AssignmentType { get; set; } = "subject_teacher";
    public bool IsActive { get; set; } = true;

    public Teacher Teacher { get; set; } = null!;
}
