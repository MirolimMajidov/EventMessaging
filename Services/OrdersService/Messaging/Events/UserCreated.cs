using EventBus.RabbitMQ.Subscribers.Models;

namespace OrdersService.Messaging.Events;

public class UserCreated : SubscribeEvent
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}