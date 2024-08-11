using EventStore.Models;
using EventStore.Outbox.Models;

namespace EventStore.Outbox;

public interface IEventSender
{
    /// <summary>
    /// To store sending an event to the database
    /// </summary>
    /// <param name="event">Event to send</param>
    /// <typeparam name="eventProvider">Provider type of sending event</typeparam>
    /// <typeparam name="eventPath">Path of event. It can be event name or routing kew or any other thing depend on event type</typeparam>
    /// <typeparam name="TSendEvent">Event type that must implement from the ISendEvent</typeparam>
    public void Send<TSendEvent>(TSendEvent @event, EventProviderType eventProvider, string eventPath) where TSendEvent : ISendEvent;
}