using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Publishers.Models;

namespace EventBus.RabbitMQ.Publishers;

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
    /// <param name="eventPublisherOptions">The eventPublisherOptions specific to the publisher, if any.</param>
    public void AddPublisher<TPublisher>(Action<EventPublisherOptions> eventPublisherOptions = null)
        where TPublisher : class, IEventPublisher
    {
        _publisherManager.AddPublisher<TPublisher>(eventPublisherOptions);
    }
}