using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Webhooks.Domain;

public sealed class WebhookSubscription : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public string EventTypes { get; set; } = "*";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public sealed class WebhookDelivery
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid SubscriptionId { get; set; }
    public Guid? TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string Status { get; set; } = WebhookDeliveryStatuses.Pending;
    public int StatusCode { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public WebhookSubscription? Subscription { get; set; }
}

public static class WebhookDeliveryStatuses
{
    public const string Pending = "pending";
    public const string Delivered = "delivered";
    public const string Failed = "failed";
}
