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
    /// <param name="eventPath">Event path of publishing an event. It can be pouting key, URL, or different value depend on provider type.</param>
    Task Publish(TSendEvent @event, string eventPath);
}