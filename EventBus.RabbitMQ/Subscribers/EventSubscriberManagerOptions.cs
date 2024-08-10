using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Subscribers.Models;

namespace EventBus.RabbitMQ.Subscribers;

public class EventSubscriberManagerOptions
{
    private readonly EventSubscriberManager _subscriberManager;

    internal EventSubscriberManagerOptions(EventSubscriberManager subscriberManager)
    {
        _subscriberManager = subscriberManager;
    }

    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="options">The options specific to the subscriber, if any.</param>
    /// <typeparam name="TEvent">Event which we want to subscribe</typeparam>
    /// <typeparam name="TEventHandler">Handler class of the event which we want to receive event</typeparam>
    public void AddSubscriber<TEvent, TEventHandler>(Action<EventSubscriberOptions> options = null)
        where TEvent : class, IEventSubscriber
        where TEventHandler : class, IEventSubscriberReceiver<TEvent>
    {
        _subscriberManager.AddSubscriber<TEvent, TEventHandler>(options);
    }
}