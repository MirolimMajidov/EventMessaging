using System.Text.Json.Serialization;

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
    
    /// <summary>
    /// Gets or sets the header data of the event.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, object> Headers { get; }
}