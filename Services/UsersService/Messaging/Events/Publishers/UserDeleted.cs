using EventBus.RabbitMQ.Publishers;
using EventStore.Models.Outbox;

namespace UsersService.Messaging.Events.Publishers;

public class UserDeleted : EventPublisher, ISendEvent
{
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
}