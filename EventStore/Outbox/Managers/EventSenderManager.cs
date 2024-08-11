using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Models;
using EventStore.Outbox.Models;
using EventStore.Outbox.Repositories;
using Microsoft.Extensions.Logging;

namespace EventStore.Outbox.Managers;

internal class EventSenderManager : IEventSenderManager
{
    private readonly IOutboxRepository _repository;
    private readonly ILogger<EventSenderManager> _logger;

    public EventSenderManager(IOutboxRepository repository, ILogger<EventSenderManager> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public bool Send<TSendEvent>(TSendEvent @event, EventProviderType eventProvider, string eventPath)
        where TSendEvent : ISendEvent
    {
        var eventName = @event.GetType().Name;
        try
        {
            var _event = new OutboxEvent()
            {
                Id = @event.EventId,
                Provider = eventProvider.ToString(),
                EventName = @event.GetType().Name,
                EventPath = eventPath,
            };

            if (@event is IHasHeaders hasHeaders)
            {
                if (hasHeaders.Headers?.Any() == true)
                    _event.Headers = SerializeData(hasHeaders.Headers);
                hasHeaders.Headers = null;
            }

            if (@event is IHasAdditionalData hasAdditionalData)
            {
                if (hasAdditionalData.AdditionalData?.Any() == true)
                    _event.AdditionalData = SerializeData(hasAdditionalData.AdditionalData);
                hasAdditionalData.AdditionalData = null;
            }

            _event.Payload = SerializeData(@event);

            var successfullyInserted = _repository.InsertEvent(_event);
            if(!successfullyInserted)
                _logger.LogWarning("The {EventType} event type with the {EventId} id is already added to the table of Outbox.", eventName, @event.EventId);

            return successfullyInserted;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while entering the {EventType} event type with the {EventId} id to the table of Outbox.",  eventName, @event.EventId);
            throw;
        }
    }

    private static readonly JsonSerializerOptions _serializerSettings = new JsonSerializerOptions
        { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    static string SerializeData<TValue>(TValue data)
    {
        return JsonSerializer.Serialize(data, _serializerSettings);
    }
}