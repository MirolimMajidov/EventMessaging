using EventStore.Inbox.Models;
using EventStore.Models;

namespace EventStore.Inbox.Managers;

public interface IEventReceiverManager
{
    /// <summary>
    /// To store receiving an event to the database
    /// </summary>
    /// <param name="event">Event to send</param>
    /// <typeparam name="eventProvider">Provider type of sending event</typeparam>
    /// <typeparam name="eventPath">Path of event. It can be event name or routing kew or any other thing depend on event type</typeparam>
    /// <typeparam name="TReceiveEvent">Event type that must implement from the IEventReceiverManager</typeparam>
    public void Received<TReceiveEvent>(TReceiveEvent @event, EventProviderType eventProvider, string eventPath) where TReceiveEvent : IReceiveEvent;
}