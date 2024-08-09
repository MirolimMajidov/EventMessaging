using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Models;
using EventStore.Models.Outbox;
using EventStore.Repositories.Outbox;

namespace EventStore.Outbox;

internal class EventSender : IEventSender
{
    private readonly IOutboxRepository _repository;

    public EventSender(IOutboxRepository repository)
    {
        _repository = repository;
    }

    public void Send<TSendEvent>(TSendEvent @event, EventProviderType eventProviderType, string eventPath,
        JsonNamingPolicy namingPolicy = null) where TSendEvent : ISendEvent
    {
        var settings = new JsonSerializerOptions
            { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        if (namingPolicy is not null)
            settings.PropertyNamingPolicy = namingPolicy;
        var messageBody = JsonSerializer.Serialize(@event, settings);

        var metaData = new EventMetadata();
        if (@event is IHasHeaders hasHeaders)
            metaData.Headers = hasHeaders.Headers;

        if (@event is IHasAdditionalData hasAdditionalData)
            metaData.AdditionalData = hasAdditionalData.AdditionalData;

        var messageHeaders = JsonSerializer.Serialize(metaData, settings);
        var _event = new OutboxEvent()
        {
            EventPath = eventPath,
            Payload = messageBody,
            Metadata = messageHeaders
        };
        _repository.InsertEvent(_event);
    }
}