using EventBus.RabbitMQ.Subscribers.Models;

namespace OrdersService.Messaging.Events;

public class UserCreated : EventSubscriber
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}