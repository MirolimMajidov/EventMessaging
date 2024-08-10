namespace EventStore.Models.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type RabbitMQ
/// </summary>
public interface IReceiveRabbitMQEvent<TReceiveEvent> : IReceiveEvent<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
    
}