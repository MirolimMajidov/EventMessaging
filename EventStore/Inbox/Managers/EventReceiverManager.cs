using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Inbox.Models;
using EventStore.Inbox.Repositories;
using EventStore.Models;
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

    public bool Received<TReceiveEvent>(TReceiveEvent @event, EventProviderType eventProvider, string eventPath)
        where TReceiveEvent : IReceiveEvent
    {
        var eventName = @event.GetType().Name;
        try
        {
            var _event = new InboxEvent()
            {
                Id = @event.EventId,
                Provider = eventProvider.ToString(),
                EventName = eventName,
                EventPath = eventPath,
            };

            if (@event is IHasHeaders hasHeaders)
            {
                if (hasHeaders.Headers?.Any() == true)
                    _event.Headers = SerializeData(hasHeaders.Headers);
                hasHeaders.Headers = null;
            }

            _event.Payload = SerializeData(@event);

            var successfullyInserted = _repository.InsertEvent(_event);
            if(!successfullyInserted)
                _logger.LogWarning("The {EventType} event type with the {EventId} id is already added to the table of Inbox.", eventName, @event.EventId);

            return successfullyInserted;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while entering the {EventType} event type with the {EventId} id to the table of Inbox.",  eventName, @event.EventId);
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