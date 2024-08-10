using EventBus.RabbitMQ.Publishers;
using EventBus.RabbitMQ.Publishers.Models;
using EventStore.Models.Inbox;

namespace EventBus.RabbitMQ.Subscribers.Models;

/// <summary>
/// Base interface for all subscriber classes
/// </summary>
public interface IEventSubscriber : IBaseEvent, IReceiveEvent
{
}