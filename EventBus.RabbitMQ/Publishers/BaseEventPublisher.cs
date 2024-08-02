namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base class for all publisher classes
/// </summary>
public abstract class BaseEventPublisher : IEventPublisher
{
    public Guid EventId { get; internal set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; internal set; } = DateTime.Now;
}