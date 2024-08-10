using EventStore.Models.Outbox;

namespace EventStore.Outbox;

/// <summary>
/// Manager of event publisher
/// </summary>
public interface IEventPublisherManager
{
    /// <summary>
    /// For executing a publisher of event
    /// </summary>
    /// <param name="event">Event to publish</param>
    /// <param name="providerName">Provider name of event</param>
    /// <returns>Return true if it executes successfully</returns>
    Task<bool> ExecuteEventPublisher(IOutboxEvent @event, string providerName);
}