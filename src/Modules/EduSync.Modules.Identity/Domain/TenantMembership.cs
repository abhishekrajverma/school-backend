namespace EduSync.Modules.Identity.Domain;

public sealed class TenantMembership
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = UserRoles.Admin;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }

    public User User { get; set; } = null!;
}
