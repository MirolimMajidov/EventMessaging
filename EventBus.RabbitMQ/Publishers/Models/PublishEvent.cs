using System.Text.Json.Serialization;

namespace EventBus.RabbitMQ.Publishers.Models;

/// <summary>
/// Base class for all publisher classes
/// </summary>
public abstract record PublishEvent : IPublishEvent
{
    public PublishEvent(Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }

    public Guid Id { get; }

    public DateTime CreatedAt { get; }

    [JsonIgnore]
    public Dictionary<string, string> Headers { get; set; }
}