using EduSync.Modules.Notifications.Application;

namespace EduSync.Infrastructure.Realtime;

public sealed class NoOpNotificationRealtimePublisher : INotificationRealtimePublisher
{
    public Task PublishCreatedAsync(
        string tenantExternalId,
        string targetAudience,
        NotificationDto notification,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
