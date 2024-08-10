namespace EventStore.Models.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type RabbitMQ
/// </summary>
public interface IPublishRabbitMQEvent<TSendEvent> : IPublishEvent<TSendEvent>
    where TSendEvent : class, ISendEvent
{
    
}