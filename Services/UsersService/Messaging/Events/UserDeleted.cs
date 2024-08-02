using EventBus.RabbitMQ.Publishers;

namespace UsersService.Messaging.Events;

public class UserDeleted : EventPublisher
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; }
}