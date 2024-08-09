namespace EventBus.RabbitMQ.Subscribers;

internal interface IEventSubscriberManager
{
    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="options">The options specific to the subscriber, if any.</param>
    /// <typeparam name="TEvent">Event which we want to subscribe</typeparam>
    /// <typeparam name="TEventHandler">Handler class of the event which we want to receive event</typeparam>
    public void AddSubscriber<TEvent, TEventHandler>(Action<EventSubscriberOptions> options = null)
        where TEvent : class, IEventSubscriber
        where TEventHandler : class, IEventSubscriberHandler<TEvent>;

    /// <summary>
    /// Creating and register each unique a queue for different virtual host and start receiving events
    /// </summary>
    public void CreateConsumerForEachQueueAndStartReceivingEvents();
}