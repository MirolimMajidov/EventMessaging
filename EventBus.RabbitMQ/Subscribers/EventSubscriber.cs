using System.Text.Json.Serialization;

namespace EventBus.RabbitMQ.Subscribers;

/// <summary>
/// Base class for all subscriber classes
/// </summary>
public abstract class EventSubscriber : IEventSubscriber
{
    public Guid EventId { get; }

    public DateTime CreatedAt { get; }
    
    [JsonIgnore]
    public Dictionary<string, string> Headers { get; set; }
}