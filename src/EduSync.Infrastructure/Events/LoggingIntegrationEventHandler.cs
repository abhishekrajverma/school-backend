using Microsoft.Extensions.Logging;

namespace EduSync.Infrastructure.Events;

public sealed class LoggingIntegrationEventHandler(ILogger<LoggingIntegrationEventHandler> logger)
    : IIntegrationEventHandler
{
    public bool CanHandle(string eventType) => true;

    public Task HandleAsync(string eventType, string payload, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Integration event {EventType} payload {Payload}", eventType, payload);
        return Task.CompletedTask;
    }
}
