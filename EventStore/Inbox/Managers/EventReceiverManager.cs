using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Exceptions;
using EventStore.Inbox.Models;
using EventStore.Inbox.Repositories;
using EventStore.Models;
using EventStore.Outbox.Models;
using Microsoft.Extensions.Logging;

namespace EventStore.Inbox.Managers;

internal class EventReceiverManager : IEventReceiverManager
{
    private readonly IInboxRepository _repository;
    private readonly ILogger<EventReceiverManager> _logger;

    public EventReceiverManager(IInboxRepository repository, ILogger<EventReceiverManager> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public bool Received<TReceiveEvent>(TReceiveEvent receivedEvent, string eventPath, EventProviderType eventProvider)
        where TReceiveEvent : IReceiveEvent
    {
        var receivedEventType = receivedEvent.GetType().Name;
        try
        {
            string headers = null;
            if (receivedEvent is IHasHeaders hasHeaders)
            {
                if (hasHeaders.Headers?.Any() == true)
                    headers = SerializeHeadersData(hasHeaders.Headers);
                hasHeaders.Headers = null;
            }

            string additionalData = null;
            if (receivedEvent is IHasAdditionalData hasAdditionalData)
            {
                if (hasAdditionalData.AdditionalData?.Any() == true)
                    additionalData = SerializeHeadersData(hasAdditionalData.AdditionalData);
                hasAdditionalData.AdditionalData = null;
            }

            var payload = SerializeData(receivedEvent);

            return Received(receivedEventType, eventPath, receivedEvent.EventId, eventProvider, payload, headers,
                additionalData);
        }
        catch (Exception e) when (e is not EventStoreException)
        {
            _logger.LogError(e,
                "Error while serializing data of the {EventType} received event with the {EventId} id to store to the the table of Inbox.",
                receivedEventType, receivedEvent.EventId);
            throw;
        }

        static string SerializeHeadersData<TValue>(TValue data)
        {
            return JsonSerializer.Serialize(data);
        }
    }

    public bool Received<TReceiveEvent>(TReceiveEvent receivedEvent, string eventPath, EventProviderType eventProvider,
        string headers, string additionalData = null)
        where TReceiveEvent : IReceiveEvent
    {
        var receivedEventType = receivedEvent.GetType().Name;
        try
        {
            var payload = SerializeData(receivedEvent);

            return Received(receivedEventType, eventPath, receivedEvent.EventId, eventProvider, payload, headers,
                additionalData);
        }
        catch (Exception e) when (e is not EventStoreException)
        {
            _logger.LogError(e,
                "Error while serializing data of the {EventType} received event with the {EventId} id to store to the the table of Inbox.",
                receivedEventType, receivedEvent.EventId);
            throw;
        }
    }

    public bool Received(string eventName, string eventPath, Guid eventId, EventProviderType eventProvider,
        string payload, string headers, string additionalData = null)
    {
        try
        {
            var inboxEvent = new InboxEvent
            {
                Id = eventId,
                Provider = eventProvider.ToString(),
                EventName = eventName,
                EventPath = eventPath,
                Payload = payload,
                Headers = headers,
                AdditionalData = additionalData
            };

            var successfullyInserted = _repository.InsertEvent(inboxEvent);
            if (!successfullyInserted)
                _logger.LogWarning(
                    "The {EventType} event type with the {EventId} id is already added to the table of Inbox.",
                    eventName, eventId);

            return successfullyInserted;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error while entering the {EventType} event type with the {EventId} id to the table of Inbox.",
                eventName, eventId);
            throw;
        }
    }

    private static readonly JsonSerializerOptions SerializerSettings =
        new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    private static string SerializeData<TValue>(TValue data)
    {
        return JsonSerializer.Serialize(data, data.GetType(), SerializerSettings);
    }
}