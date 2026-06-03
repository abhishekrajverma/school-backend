namespace EduSync.Infrastructure.Events;

public sealed record IntegrationEvent(
    string EventType,
    Guid? TenantId,
    string Payload,
    string? Region = null,
    string? CorrelationId = null);
