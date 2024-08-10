using EventBus.RabbitMQ.Subscribers.Models;

namespace PaymentsService.Messaging.Events.Subscribers;

public class UserCreated : EventSubscriber
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}