using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Identity.Domain;

public sealed class User : AuditableEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.Admin;
    public bool IsActive { get; set; } = true;

    public ICollection<TenantMembership> Memberships { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
