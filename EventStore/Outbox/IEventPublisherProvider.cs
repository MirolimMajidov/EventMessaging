using EventStore.Models;

namespace EventStore.Outbox;

public interface IEventPublisherProvider
{
    /// <summary>
    /// Event provider type to handler necessary events
    /// </summary>
    EventProviderType EventProviderType { get; }
}