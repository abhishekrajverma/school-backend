using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Jobs.Domain;

public sealed class JobExecution : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = "running";
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Message { get; set; }
    public int ItemsProcessed { get; set; }
}
