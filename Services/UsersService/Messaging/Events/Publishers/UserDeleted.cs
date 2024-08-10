using EventBus.RabbitMQ.Publishers.Models;

namespace UsersService.Messaging.Events.Publishers;

public class UserDeleted : EventPublisher
{
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
}