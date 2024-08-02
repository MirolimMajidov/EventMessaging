using EventBus.RabbitMQ.Publishers;
using EventBus.RabbitMQ.Subscribers;

namespace EventBus.RabbitMQ.Configurations;

public class RabbitMQSettings
{
    /// <summary>
    /// The default settings for connecting to the RabbitMQ server. If a publisher or subscriber does not have specific settings, these default settings will be used.
    /// </summary>
    public RabbitMQOptions DefaultSettings { get; set; }

    /// <summary>
    /// A dictionary where each key represents a publisher and its associated options for connecting to the RabbitMQ server and publishing messages. If no specific settings are provided, it will use the default options.
    /// </summary>
    public Dictionary<string, EventPublisherOptions> Publishers { get; set; } = new();

    /// <summary>
    /// A dictionary where each key represents a subscriber and its associated options for connecting to the RabbitMQ server and receiving messages. If no specific settings are provided, it will use the default options.
    /// </summary>
    public Dictionary<string, EventSubscriberOptions> Subscribers { get; set; } = new();
}