using EduSync.Infrastructure.Realtime;
using EduSync.Modules.Notifications.Application;
using Microsoft.AspNetCore.SignalR;

namespace EduSync.Api.SignalR;

public sealed class SignalRNotificationRealtimePublisher(IHubContext<NotificationsHub> hub) : INotificationRealtimePublisher
{
    public async Task PublishCreatedAsync(
        string tenantExternalId,
        string targetAudience,
        NotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        var clients = string.IsNullOrWhiteSpace(targetAudience)
            || targetAudience.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? hub.Clients.Group(NotificationsHub.TenantGroup(tenantExternalId))
            : hub.Clients.Group(NotificationsHub.AudienceGroup(tenantExternalId, targetAudience));

        await clients.SendAsync(NotificationsHub.EventCreated, notification, cancellationToken);
    }
}
