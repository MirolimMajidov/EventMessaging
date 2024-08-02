namespace EventBus.RabbitMQ.Subscribers;

/// <summary>
/// Base class for all subscriber classes
/// </summary>
public abstract class EventSubscriber : IEventSubscriber
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.Now;
}