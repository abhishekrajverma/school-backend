using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Students.Domain;

public sealed class StudentEnrollment : BranchEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Guid AcademicYearId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public string EnrollmentStatus { get; set; } = EnrollmentStatuses.Enrolled;
    public Guid? PromotedFromEnrollmentId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Student Student { get; set; } = null!;
}
