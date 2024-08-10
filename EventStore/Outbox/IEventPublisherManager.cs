namespace EventStore.Outbox;

/// <summary>
/// Manager of event publisher
/// </summary>
internal interface IEventPublisherManager
{
    /// <summary>
    /// For publishing unprocessed events
    /// </summary>
    Task ExecuteUnprocessedEvents(CancellationToken stoppingToken);
}