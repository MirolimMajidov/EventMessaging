using EventBus.RabbitMQ.Subscribers.Models;

namespace PaymentsService.Messaging.Events.Subscribers;

public class UserCreated : SubscribeEvent
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}