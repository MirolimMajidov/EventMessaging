namespace EventBus.RabbitMQ.Subscribers;

internal class EventConsumerService : IEventConsumerService
{
    /// <summary>
    /// Dictionary collection to store all event and event handler information
    /// </summary>
    private readonly List<(Type EventType, Type EventHandlerType, EventSubscriberOptions EventSettings)>
        _subscribers = new();

    private readonly EventSubscriberOptions _defaultEventSubscriberOptions;
    
    public EventConsumerService(EventSubscriberOptions defaultEventSubscriberOptions)
    {
        _defaultEventSubscriberOptions = defaultEventSubscriberOptions;
    }

    public void AddSubscriber((Type, Type, EventSubscriberOptions) eventInfo)
    {
        _subscribers.Add(eventInfo);
    }
}