namespace EventStore.Models.Outbox.Providers;

/// <summary>
/// An interface for determine a publisher of events. It may use for Unknown type.
/// </summary>
public interface IPublishEvent<TSendEvent>
    where TSendEvent :  class, ISendEvent
{
    /// <summary>
    /// For publishing an event 
    /// </summary>
    /// <param name="event">Publishing an event</param>
    /// <param name="eventPath">Event path of publishing an event. It can be routing key, URL, or different value depend on provider type.</param>
    /// <returns>Return true if it executes successfully</returns>
    Task<bool> Publish(TSendEvent @event, string eventPath);
}