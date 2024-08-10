using System.Text.Json.Serialization;
using EventStore.Models;
using EventStore.Models.Outbox;

namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base interface for all publisher and subscriber interface
/// </summary>
public interface IBaseEvent : ISendEvent, IHasHeaders
{
    /// <summary>
    /// Created time of event
    /// </summary>
    public DateTime CreatedAt { get; }
}