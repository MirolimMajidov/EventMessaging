using EventStore.Models;

namespace EventBus.RabbitMQ.Publishers.Models;

/// <summary>
/// Base interface for all publisher and subscriber interface
/// </summary>
public interface IBaseEvent : IHasHeaders
{
    /// <summary>
    /// Created time of event
    /// </summary>
    public DateTime CreatedAt { get; }
}