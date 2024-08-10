namespace EventStore.Models.Outbox;

/// <summary>
/// An interface for determine a publish of events
/// </summary>
public interface IPublishEvent<TSendEvent>
    where TSendEvent :  class, ISendEvent
{
    /// <summary>
    /// To publish a event 
    /// </summary>
    /// <param name="event">Publishing an event</param>
    Task Publish(TSendEvent @event);
}