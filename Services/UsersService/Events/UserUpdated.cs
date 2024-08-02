using EventBus.RabbitMQ.Publishers;

namespace UsersService.Events;

public class UserUpdated : BaseEventPublisher
{
    public Guid UserId { get; set; }
    
    public string OldUserName { get; set; }
    
    public string NewUserName { get; set; }
}