using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Audit.Domain;

public sealed class AuditLogEntry : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? Region { get; set; }
    public DateTime OccurredAt { get; set; }
}
