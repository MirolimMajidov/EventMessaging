using EventBus.RabbitMQ.Subscribers.Models;

namespace OrdersService.Messaging.Events;

public record UserCreated : SubscribeEvent
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}