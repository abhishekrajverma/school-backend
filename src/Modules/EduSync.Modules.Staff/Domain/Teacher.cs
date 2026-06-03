using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Staff.Domain;

public sealed class Teacher : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public int ExperienceYears { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal Salary { get; set; }
    public DateOnly? JoiningDate { get; set; }
    public string Status { get; set; } = "active";
    public string? ClassesJson { get; set; }
    public string? AvatarUrl { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
