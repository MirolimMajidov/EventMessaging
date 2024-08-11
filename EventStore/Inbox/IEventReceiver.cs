using EventStore.Inbox.Models;
using EventStore.Models;

namespace EventStore.Inbox;

public interface IEventReceiver
{
    /// <summary>
    /// To store receiving an event to the database
    /// </summary>
    /// <param name="event">Event to send</param>
    /// <typeparam name="eventProvider">Provider type of sending event</typeparam>
    /// <typeparam name="eventPath">Path of event. It can be event name or routing kew or any other thing depend on event type</typeparam>
    /// <typeparam name="TReceiveEvent">Event type that must implement from the IEventReceiver</typeparam>
    public void Receive<TReceiveEvent>(TReceiveEvent @event, EventProviderType eventProvider, string eventPath) where TReceiveEvent : IReceiveEvent;
}