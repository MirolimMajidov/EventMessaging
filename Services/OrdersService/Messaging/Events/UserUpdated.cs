using EventBus.RabbitMQ.Subscribers.Models;

namespace OrdersService.Messaging.Events;

public record UserUpdated : SubscribeEvent
{
    public Guid UserId { get; set; }
    
    public string OldUserName { get; set; }
    
    public string NewUserName { get; set; }
}