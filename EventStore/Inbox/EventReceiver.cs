using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Models;
using EventStore.Models.Inbox;
using EventStore.Repositories.Inbox;

namespace EventStore.Inbox;

internal class EventReceiver : IEventReceiver
{
    private readonly IInboxRepository _repository;

    public EventReceiver(IInboxRepository repository)
    {
        _repository = repository;
    }

    public void Receive<TReceiveEvent>(TReceiveEvent @event, EventProviderType eventProvider, string eventPath)
        where TReceiveEvent : IReceiveEvent
    {
        var _event = new InboxEvent()
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

        _repository.InsertEvent(_event);
    }

    private static readonly JsonSerializerOptions _serializerSettings = new JsonSerializerOptions
        { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    static string SerializeData<TValue>(TValue data)
    {
        return JsonSerializer.Serialize(data, _serializerSettings);
    }
}