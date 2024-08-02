using EventBus.RabbitMQ.Publishers;

namespace EventBus.RabbitMQ.Subscribers;

/// <summary>
/// Base interface for all subscriber classes
/// </summary>
public interface IEventSubscriber : IBaseEvent
{
}