namespace EventStore.Outbox;

/// <summary>
/// Manager of event publisher
/// </summary>
public interface IEventPublisherManager
{
    /// <summary>
    /// For publishing unprocessed events
    /// </summary>
    Task PublisherUnprocessedEvents(CancellationToken stoppingToken);
}