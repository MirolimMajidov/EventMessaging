using EventBus.RabbitMQ.Publishers;

namespace UsersService.Messaging.Events;

public class UserCreated : EventPublisher
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; }
}