using System.Text.Json.Serialization;
using EventBus.RabbitMQ.Publishers;
using EventStore.Models;
using EventStore.Models.Outbox;

namespace UsersService.Messaging.Events.Publishers;

public class UserCreated : EventPublisher, ISendEvent, IHasHeaders, IHasAdditionalData
{
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
    
    [JsonIgnore]
    public Dictionary<string, string> AdditionalData { get; set; }
}