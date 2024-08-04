using EventBus.RabbitMQ.Publishers;

namespace UsersService.Messaging.Events;

public class UserDeleted : EventPublisher
{
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
}