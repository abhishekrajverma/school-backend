namespace EduSync.Infrastructure.Events;

public interface IIntegrationEventHandler
{
    bool CanHandle(string eventType);
    Task HandleAsync(
        string eventType,
        string payload,
        Guid? tenantId,
        CancellationToken cancellationToken = default);
}
