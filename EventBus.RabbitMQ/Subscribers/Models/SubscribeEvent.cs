using System.Text.Json.Serialization;

namespace EventBus.RabbitMQ.Subscribers.Models;

/// <summary>
/// Base class for all subscriber classes
/// </summary>
public abstract record SubscribeEvent : ISubscribeEvent
{
    public Guid Id { get; init; }

    public DateTime CreatedAt { get; init; }

    [JsonIgnore] 
    public Dictionary<string, string> Headers { get; set; }
}