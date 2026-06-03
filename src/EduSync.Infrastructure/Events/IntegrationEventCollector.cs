namespace EduSync.Infrastructure.Events;

public sealed class IntegrationEventCollector : IIntegrationEventCollector
{
    private readonly List<IntegrationEvent> _events = [];

    public void Add(IntegrationEvent integrationEvent) => _events.Add(integrationEvent);

    public IReadOnlyList<IntegrationEvent> Drain()
    {
        if (_events.Count == 0)
        {
            return [];
        }

        var copy = _events.ToList();
        _events.Clear();
        return copy;
    }
}
