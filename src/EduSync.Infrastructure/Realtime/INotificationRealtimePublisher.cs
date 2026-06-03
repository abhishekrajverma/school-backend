using EduSync.Modules.Notifications.Application;

namespace EduSync.Infrastructure.Realtime;

public interface INotificationRealtimePublisher
{
    Task PublishCreatedAsync(
        string tenantExternalId,
        string targetAudience,
        NotificationDto notification,
        CancellationToken cancellationToken = default);
}
