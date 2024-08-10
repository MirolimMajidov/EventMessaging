using System.Text.Json.Serialization;

namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base class for all publisher classes
/// </summary>
public abstract class EventPublisher : IEventPublisher
{
    public EventPublisher(Guid? id = null)
    {
        EventId = id ?? Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }

    public Guid EventId { get; }

    public DateTime CreatedAt { get; }

    [JsonIgnore]
    public Dictionary<string, string> Headers { get; set; }
}