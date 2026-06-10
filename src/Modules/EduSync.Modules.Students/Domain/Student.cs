using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Students.Domain;

public sealed partial class Student : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Address { get; set; }
    public string AdmissionNo { get; set; } = string.Empty;
    public string LifecycleStatus { get; set; } = LifecycleStatuses.Active;
    public string? AvatarUrl { get; set; }
    public Guid? AdmissionApplicationId { get; set; }

    public ICollection<StudentEnrollment> Enrollments { get; set; } = [];

    public string FullName => $"{FirstName} {LastName}".Trim();
}
