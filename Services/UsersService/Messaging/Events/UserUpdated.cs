using EventBus.RabbitMQ.Publishers;

namespace UsersService.Messaging.Events;

public class UserUpdated : EventPublisher
{
    public Guid UserId { get; set; }
    
    public string OldUserName { get; set; }
    
    public string NewUserName { get; set; }
}