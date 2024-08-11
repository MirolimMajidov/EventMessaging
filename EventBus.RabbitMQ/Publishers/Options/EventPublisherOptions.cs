using EventBus.RabbitMQ.Configurations;

namespace EventBus.RabbitMQ.Publishers.Options;

public class EventPublisherOptions : BaseEventOptions
{
    /// <summary>
    /// The name of the event. By default, it will get an event name.
    /// </summary>
    public string EventTypeName { get; set; }
}