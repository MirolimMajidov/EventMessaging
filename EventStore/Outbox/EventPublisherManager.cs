using System.Text;
using System.Text.Json;
using EventStore.Models;
using EventStore.Models.Outbox;
using EventStore.Models.Outbox.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStore.Outbox;

/// <summary>
/// Manager of events publisher
/// </summary>
public class EventPublisherManager : IEventPublisherManager
{
    private readonly ILogger<EventPublisherManager> _logger;

    private readonly Dictionary<string, (Type eventType, Type eventHandlerType, string providerType, bool
        hasHeaders, bool hasAdditionalData)> _publishers;

    public EventPublisherManager(ILogger<EventPublisherManager> logger)
    {
        _logger = logger;
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

            _publishers.Add(eventName,
                (typeOfEventSender, typeOfEventPublisher, providerType.ToString(), hasHeaders, hasAdditionalData));
        }
    }

    private const string PublisherMethodName = nameof(IPublishEvent<ISendEvent>.Publish);

    public async Task<bool> ExecuteEventPublisher(IOutboxEvent @event, string providerType, IServiceScope serviceScope)
    {
        try
        {
            if (_publishers.TryGetValue(@event.EventName,
                    out (Type eventType, Type eventHandlerType, string providerType, bool hasHeaders, bool
                    hasAdditionalData) info))
            {
                if (@event.Provider == providerType)
                    _logger.LogTrace("Executing the {EventType} outbox event with ID {EventId} to publish.",
                        @event.EventName, @event.Id);
                else
                    _logger.LogError(
                        "The {EventType} outbox event with ID {EventId} requested to publish with {ProviderType} provider, but that is configured to publish with the {ConfiguredProviderType} provider.",
                        @event.EventName, @event.Id, @event.Provider, providerType);

                var eventToPublish = JsonSerializer.Deserialize(@event.Payload, info.eventType) as ISendEvent;
                if (info.hasHeaders && @event.Headers is not null)
                    ((IHasHeaders)eventToPublish).Headers =
                        JsonSerializer.Deserialize<Dictionary<string, object>>(@event.Headers);

                if (info.hasAdditionalData && @event.AdditionalData is not null)
                    ((IHasAdditionalData)eventToPublish).AdditionalData =
                        JsonSerializer.Deserialize<Dictionary<string, object>>(@event!.AdditionalData);

                var eventHandlerSubscriber = serviceScope.ServiceProvider.GetRequiredService(info.eventHandlerType);

                var publisherMethod = info.eventHandlerType.GetMethod(PublisherMethodName);
                var result =
                    await (Task<bool>)publisherMethod.Invoke(eventHandlerSubscriber,
                        [eventToPublish, @event.EventPath]);
                return result;
            }
            else
            {
                _logger.LogWarning(
                    "No publish provider configured for the {EventType} outbox event with ID: {EventId}.",
                    @event.EventName, @event.Id);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while publishing event with ID: {EventId}", @event.Id);
            throw;
        }

        return false;
    }
}