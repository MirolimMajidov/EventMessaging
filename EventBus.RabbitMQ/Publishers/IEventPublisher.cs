namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base interface for all publisher classes
/// </summary>
public interface IEventPublisher
{
    public Guid EventId { get; }
    
    public DateTime CreatedAt { get; }
}