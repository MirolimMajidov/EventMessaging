using EventStore.Outbox.Models;

namespace EventStore.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type RabbitMQ
/// </summary>
public interface IRabbitMqEventSender<TSendEvent> : IEventSender<TSendEvent>
    where TSendEvent : class, ISendEvent
{
    
}