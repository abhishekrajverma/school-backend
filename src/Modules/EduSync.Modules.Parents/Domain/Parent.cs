using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Parents.Domain;

public sealed class Parent : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Occupation { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "active";
    public string? ChildrenJson { get; set; }
    public string? StudentIdsJson { get; set; }
    public string? AvatarUrl { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
