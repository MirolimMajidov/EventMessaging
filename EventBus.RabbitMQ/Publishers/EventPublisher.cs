using System.Collections;

namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base class for all publisher classes
/// </summary>
public abstract class EventPublisher : IEventPublisher
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.Now;

    private Dictionary<string, object> Headers { get; set; }

    public bool TryAddHeader(string name, object value)
    {
        Headers ??= new();
        return Headers.TryAdd(name, value);
    }

    public IDictionary<string, object> GetHeaders()
    {
        return Headers;
    }
}