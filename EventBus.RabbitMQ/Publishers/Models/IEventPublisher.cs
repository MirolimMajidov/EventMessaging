using EventStore.Outbox.Models;

namespace EventBus.RabbitMQ.Publishers.Models;

/// <summary>
/// Base interface for all publisher classes
/// </summary>
public interface IEventPublisher : IBaseEvent, ISendEvent
{
}