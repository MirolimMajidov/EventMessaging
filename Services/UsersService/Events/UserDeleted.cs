using EventBus.RabbitMQ.Publishers;

namespace UsersService.Events;

public class UserDeleted : BaseEventPublisher
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; }
}