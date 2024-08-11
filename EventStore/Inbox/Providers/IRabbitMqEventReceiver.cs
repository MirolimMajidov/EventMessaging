using EventStore.Inbox.Models;

namespace EventStore.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type RabbitMQ
/// </summary>
public interface IRabbitMqEventReceiver<TReceiveEvent> : IEventReceiver<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
    
}