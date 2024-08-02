using EventBus.RabbitMQ.Publishers;

namespace EventBus.RabbitMQ.Configurations;

public class EventPublisherManagerOptions
{
    private readonly EventPublisherManager _publisherManager;

    internal EventPublisherManagerOptions(EventPublisherManager publisherManager)
    {
        _publisherManager = publisherManager;
    }

    /// <summary>
    /// Registers a publisher.
    /// </summary>
    /// <param name="options">The options specific to the publisher, if any.</param>
    public void AddPublisher<TPublisher>(Action<RabbitMQOptions>? options = null)
        where TPublisher : class, IEventPublisher
    {
        _publisherManager.AddPublisher<TPublisher>(options);
    }
}