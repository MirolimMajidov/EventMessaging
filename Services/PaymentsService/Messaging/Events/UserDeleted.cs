using EventBus.RabbitMQ.Publishers;
using EventBus.RabbitMQ.Subscribers;

namespace PaymentsService.Messaging.Events;

public class UserDeleted : EventSubscriber
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}