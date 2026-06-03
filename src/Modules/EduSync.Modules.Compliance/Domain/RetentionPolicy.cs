using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Compliance.Domain;

public sealed class RetentionPolicy : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int RetentionDays { get; set; } = 90;
    public bool IsEnabled { get; set; } = true;
    public DateTime UpdatedAtPolicy { get; set; }
}

public static class RetentionEntityTypes
{
    public const string AuditLogs = "audit_logs";
    public const string OutboxProcessed = "outbox_processed";
    public const string WebhookDeliveries = "webhook_deliveries";
}
