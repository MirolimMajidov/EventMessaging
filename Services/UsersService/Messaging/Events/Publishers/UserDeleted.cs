using EventBus.RabbitMQ.Publishers.Models;

namespace UsersService.Messaging.Events.Publishers;

public class UserDeleted : PublishEvent
{
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
}