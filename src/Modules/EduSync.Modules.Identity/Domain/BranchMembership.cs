namespace EduSync.Modules.Identity.Domain;

public sealed class BranchMembership
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = UserRoles.Teacher;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }

    public User User { get; set; } = null!;
}
