namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base class for all publisher classes
/// </summary>
public abstract class EventPublisher : IEventPublisher
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.Now;
    
    public Dictionary<string, object> Headers { get; } = new();
}