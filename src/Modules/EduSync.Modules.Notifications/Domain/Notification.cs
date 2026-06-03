using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Notifications.Domain;

public sealed class Notification : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info";
    public string TargetAudience { get; set; } = "all";
    public DateTime SentAt { get; set; }
    public int ReadCount { get; set; }
    public int TotalRecipients { get; set; }
}
