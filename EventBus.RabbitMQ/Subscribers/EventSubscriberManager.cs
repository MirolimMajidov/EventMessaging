using EventBus.RabbitMQ.Configurations;

namespace EventBus.RabbitMQ.Subscribers;

public class EventSubscriberManager(RabbitMQOptions defaultSettings, IServiceProvider serviceProvider) //TODO: it should not be public
{
    /// <summary>
    /// Dictionary collection to store all event and event handler information
    /// </summary>
    private readonly Dictionary<string, (Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings)> _subscribers = new();

    /// <summary>
    /// List of consumers for each unique a queue for different virtual host 
    /// </summary>
    private readonly Dictionary<string, IEventConsumerService> _eventConsumers = new();

    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="options">The options specific to the subscriber, if any.</param>
    /// <typeparam name="TEvent">Event which we want to subscribe</typeparam>
    /// <typeparam name="TEventHandler">Handler class of the event which we want to receive event</typeparam>
    public void AddSubscriber<TEvent, TEventHandler>(Action<EventSubscriberOptions>? options = null)
        where TEvent : class, IEventSubscriber
        where TEventHandler : class, IEventSubscriberHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        if (_subscribers.TryGetValue(eventType.Name, out var info))
        {
            options?.Invoke(info.eventSettings);
        }
        else
        {
            var settings = defaultSettings.Clone<EventSubscriberOptions>();
            options?.Invoke(settings);

            var handlerType = typeof(TEventHandler);
            _subscribers.Add(eventType.Name, (eventType, handlerType, settings));
        }
    }

    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="typeOfSubscriber">Event type which we want to subscribe</param>
    /// <param name="typeOfHandler">Handler type of the event which we want to receive event</param>
    /// <param name="settings">Settings of subscriber</param>
    public void AddSubscriber(Type typeOfSubscriber, Type typeOfHandler, EventSubscriberOptions settings)
    {
        var subscriberName = typeOfSubscriber.Name;
        EventSubscriberOptions _settings;
        if (_subscribers.TryGetValue(subscriberName, out var info))
        {
            _settings = info.eventSettings;
        }
        else
        {
            _settings = defaultSettings.Clone<EventSubscriberOptions>();
            _subscribers.Add(subscriberName, (typeOfSubscriber, typeOfHandler, _settings));
        }

        _settings.OverwriteSettings(settings);
    }

    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="typeOfSubscriber">Event type which we want to subscribe</param>
    /// <param name="typeOfHandler">Handler type of the event which we want to receive event</param>
    public void AddSubscriber(Type typeOfSubscriber, Type typeOfHandler)
    {
        AddSubscriber(typeOfSubscriber, typeOfHandler, defaultSettings.Clone<EventSubscriberOptions>());
    }

    /// <summary>
    /// Setting an event name of subscriber if empty
    /// </summary>
    public void SetEventNameOfSubscribers()
    {
        foreach (var (subscriberName, (_, _, eventSettings)) in _subscribers)
        {
            if (string.IsNullOrEmpty(eventSettings.EventTypeName))
                eventSettings.EventTypeName = subscriberName;
        }
    }

    /// <summary>
    /// Creating and register each unique a queue for different virtual host
    /// </summary>
    public void CreateConsumerForEachQueue()
    {
        foreach (var (_, eventInfo) in _subscribers)
        {
            var consumerId = $"{eventInfo.eventSettings.VirtualHost}-{eventInfo.eventSettings.QueueName}";
            if (!_eventConsumers.TryGetValue(consumerId, out IEventConsumerService _eventConsumer))
            {
                _eventConsumer = new EventConsumerService(eventInfo.eventSettings, serviceProvider);
                _eventConsumers.Add(consumerId, _eventConsumer);
            }
            _eventConsumer.AddSubscriber(eventInfo);
        }

        foreach (var _eventConsumer in _eventConsumers)
            _eventConsumer.Value.StartAndSubscribeReceiver();
    }
}