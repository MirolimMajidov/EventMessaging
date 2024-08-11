using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Inbox.Models;
using EventStore.Inbox.Repositories;
using EventStore.Models;

namespace EventStore.Inbox.Managers;

internal class EventReceiverManager : IEventReceiverManager
{
    private readonly IInboxRepository _repository;

    public EventReceiverManager(IInboxRepository repository)
    {
        _repository = repository;
    }

    public void Received<TReceiveEvent>(TReceiveEvent @event, EventProviderType eventProvider, string eventPath)
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