using System.Text.Json.Serialization;

namespace EventBus.RabbitMQ.Subscribers.Models;

/// <summary>
/// Base class for all subscriber classes
/// </summary>
public abstract class SubscribeEvent : ISubscribeEvent
{
    public Guid EventId { get; }

    public DateTime CreatedAt { get; }
    
    [JsonIgnore]
    public Dictionary<string, string> Headers { get; set; }
}