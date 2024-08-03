using EventBus.RabbitMQ.Publishers;

namespace UsersService.Messaging.Events;

public class UserUpdated : IEventPublisher
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.Now;
    
    public Guid UserId { get; set; }
    
    public string OldUserName { get; set; }
    
    public string NewUserName { get; set; }
}