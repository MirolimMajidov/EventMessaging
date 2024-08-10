namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base class for all publisher classes
/// </summary>
public abstract class EventPublisher : IEventPublisher
{
    public EventPublisher(Guid? id = null)
    {
        EventId = id ?? Guid.NewGuid();
    }

    public Guid EventId { get; }

    public DateTime CreatedAt { get; } = DateTime.Now;

    public Dictionary<string, object> Headers { get; set; }
}