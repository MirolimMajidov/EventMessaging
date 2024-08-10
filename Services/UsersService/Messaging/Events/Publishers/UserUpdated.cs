using EventBus.RabbitMQ.Publishers;

namespace UsersService.Messaging.Events.Publishers;

public class UserUpdated : EventPublisher
{
    public Guid UserId { get; init; }
    
    public string OldUserName { get; init; }
    
    public string NewUserName { get; init; }
}