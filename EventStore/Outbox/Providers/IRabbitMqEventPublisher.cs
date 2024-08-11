using EventStore.Outbox.Models;

namespace EventStore.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type RabbitMQ
/// </summary>
public interface IRabbitMqEventPublisher<TSendEvent> : IEventPublisher<TSendEvent>
    where TSendEvent : class, ISendEvent
{
    
}