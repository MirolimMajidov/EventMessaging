using EventBus.RabbitMQ.Configurations;

namespace EventBus.RabbitMQ.Subscribers;

public class EventSubscriberOptions : BaseEventOptions
{
    /// <summary>
    /// The name of the queue to use in RabbitMQ. Default value is "DefaultQueue".
    /// </summary>
    public string? QueueName { get; set; }
    
    /// <summary>
    /// The name of the event. By default, it will get an event name.
    /// </summary>
    public string? EventTypeName { get; set; }
}