namespace EduSync.Infrastructure.Events;

public interface IIntegrationEventCollector
{
    void Add(IntegrationEvent integrationEvent);
    IReadOnlyList<IntegrationEvent> Drain();
}
