namespace EduSync.Modules.Events.Domain;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string? Region { get; set; }
    public string? CorrelationId { get; set; }
    public string Status { get; set; } = OutboxStatuses.Pending;
    public int Attempts { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public static class OutboxStatuses
{
    public const string Pending = "pending";
    public const string Processed = "processed";
    public const string Failed = "failed";
}

public static class IntegrationEventTypes
{
    public const string NotificationCreated = "notification.created";
    public const string StudentCreated = "student.created";
    public const string FeePaymentRecorded = "fee.payment.recorded";
}
