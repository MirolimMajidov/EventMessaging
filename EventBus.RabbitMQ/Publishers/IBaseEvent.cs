namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base interface for all publisher and subscriber interface
/// </summary>
public interface IBaseEvent
{
    /// <summary>
    /// Id of event
    /// </summary>
    public Guid EventId { get; }
    
    /// <summary>
    /// Created time of event
    /// </summary>
    public DateTime CreatedAt { get; }
}