namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base interface for all publisher and subscriber interface
/// </summary>
public interface IBaseEvent
{
    public Guid EventId { get; }
    
    public DateTime CreatedAt { get; }
}