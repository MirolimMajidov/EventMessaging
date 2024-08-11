using System.Text.Json.Serialization;
using EventBus.RabbitMQ.Publishers.Models;
using EventStore.Models;

namespace UsersService.Messaging.Events.Publishers;

public class UserCreated : PublishEvent, IHasAdditionalData
{
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
    
    [JsonIgnore]
    public Dictionary<string, string> AdditionalData { get; set; }
}