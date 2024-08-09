using System.Text.Json;
using EventStore.Models;
using EventStore.Models.Outbox;

namespace EventStore.Outbox;

public interface IEventSender
{
    /// <summary>
    /// To store sending an event to the database
    /// </summary>
    /// <param name="event">Event to send</param>
    /// <typeparam name="eventProviderType">Provider type of sending event</typeparam>
    /// <typeparam name="eventPath">Path of event. It can be event name or routing kew or any other thing depend on event type</typeparam>
    /// <typeparam name="namingPolicy">Naming police for serializing and deserializing properties of Event. Default value is "PascalCase". It can be one of "PascalCase", "CamelCase", "SnakeCaseLower", "SnakeCaseUpper", "KebabCaseLower", or "KebabCaseUpper".</typeparam>
    /// <typeparam name="TSendEvent">Event type that must implement from the ISendEvent</typeparam>
    public void Send<TSendEvent>(TSendEvent @event, EventProviderType eventProviderType, string eventPath,
        JsonNamingPolicy namingPolicy = null) where TSendEvent : ISendEvent;
}