using EventBus.RabbitMQ.Publishers;
using EventStore.Models;
using EventStore.Models.Outbox;

namespace UsersService.Messaging.Events;

public class UserCreated : EventPublisher, ISendEvent, IHasHeaders
{
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
}