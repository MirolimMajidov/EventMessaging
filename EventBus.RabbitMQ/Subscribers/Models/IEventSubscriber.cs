using EventBus.RabbitMQ.Publishers.Models;
using EventStore.Inbox.Models;

namespace EventBus.RabbitMQ.Subscribers.Models;

/// <summary>
/// Base interface for all subscriber classes
/// </summary>
public interface IEventSubscriber : IBaseEvent, IReceiveEvent
{
}