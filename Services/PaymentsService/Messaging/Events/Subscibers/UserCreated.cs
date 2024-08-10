using EventBus.RabbitMQ.Subscribers;

namespace PaymentsService.Messaging.Events.Subscibers;

public class UserCreated : EventSubscriber
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}