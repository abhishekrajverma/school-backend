using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Students.Domain;

public sealed class Student : TenantEntity
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
    public string ClassName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public string AdmissionNo { get; set; } = string.Empty;
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
    public string Status { get; set; } = "active";
    public string? FeeStatus { get; set; }
    public int AttendancePercent { get; set; }
    public string? AvatarUrl { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
