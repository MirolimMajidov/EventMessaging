using EventStore.Models;
using EventStore.Models.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStore.Outbox;

/// <summary>
/// Manager of events publisher
/// </summary>
public class EventPublisherManager: IEventPublisherManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPublisherManager> _logger;
    private readonly Dictionary<string, (Type eventType, Type eventHandlerType, EventProviderType providerType, bool hasHeaders, bool hasAdditionalData)> _publishers;

    public EventPublisherManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<EventPublisherManager>>();
        _publishers = new();
    }

    static Type hasHeadersType = typeof(IHasHeaders);
    static Type hasAdditionalDataType = typeof(IHasAdditionalData);
    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="typeOfEventSender">Event type which we want to use to send</param>
    /// <param name="typeOfEventPublisher">Publisher type of the event which we want to publish event</param>
    /// <param name="providerType">Provider type of event publisher</param>
    public void AddPublisher(Type typeOfEventSender, Type typeOfEventPublisher, EventProviderType providerType)
    {
        var eventName = typeOfEventSender.Name;
        if (!_publishers.ContainsKey(eventName))
        {
            var hasHeaders = hasHeadersType.IsAssignableFrom(typeOfEventSender);
            var hasAdditionalData = hasAdditionalDataType.IsAssignableFrom(typeOfEventSender);
            
            _publishers.Add(eventName, (typeOfEventSender, typeOfEventPublisher, providerType, hasHeaders, hasAdditionalData));
        }
    }

    public Task<bool> ExecuteEventPublisher(IOutboxEvent @event, string providerName)
    {
        return Task.FromResult(true);
    }
}