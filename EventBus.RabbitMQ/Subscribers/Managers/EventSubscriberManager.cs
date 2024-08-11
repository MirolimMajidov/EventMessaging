using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Subscribers.Consumers;
using EventBus.RabbitMQ.Subscribers.Models;
using EventBus.RabbitMQ.Subscribers.Options;

namespace EventBus.RabbitMQ.Subscribers.Managers;

internal class EventSubscriberManager(RabbitMQOptions defaultSettings, IServiceProvider serviceProvider)
    : IEventSubscriberManager
{
    /// <summary>
    /// Dictionary collection to store all event and event handler information
    /// </summary>
    private readonly Dictionary<string, (Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings)>
        _subscribers = new();

    /// <summary>
    /// List of consumers for each unique a queue for different virtual host 
    /// </summary>
    private readonly Dictionary<string, IEventConsumerService> _eventConsumers = new();

    public void AddSubscriber<TEvent, TEventHandler>(Action<EventSubscriberOptions> options = null)
        where TEvent : class, ISubscribeEvent
        where TEventHandler : class, IEventSubscriber<TEvent>
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
    /// <param name="subscriberSettings">Settings of subscriber</param>
    public void AddSubscriber(Type typeOfSubscriber, Type typeOfHandler, EventSubscriberOptions subscriberSettings)
    {
        var subscriberName = typeOfSubscriber.Name;
        EventSubscriberOptions settings;
        if (_subscribers.TryGetValue(subscriberName, out var info))
        {
            settings = info.eventSettings;
        }
        else
        {
            settings = defaultSettings.Clone<EventSubscriberOptions>();
            _subscribers.Add(subscriberName, (typeOfSubscriber, typeOfHandler, settings));
        }

        settings.OverwriteSettings(subscriberSettings);
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

    public void CreateConsumerForEachQueueAndStartReceivingEvents()
    {
        foreach (var (_, eventInfo) in _subscribers)
        {
            var consumerId = $"{eventInfo.eventSettings.VirtualHost}-{eventInfo.eventSettings.QueueName}";
            if (!_eventConsumers.TryGetValue(consumerId, value: out IEventConsumerService eventConsumer))
            {
                eventConsumer = new EventConsumerService(eventInfo.eventSettings, serviceProvider);
                _eventConsumers.Add(consumerId, eventConsumer);
            }

            eventConsumer.AddSubscriber(eventInfo);
        }

        foreach (var consumer in _eventConsumers)
            consumer.Value.StartAndSubscribeReceiver();
    }
}